using System.Security.Claims;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Helpers;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly AppDbContext _db;

    public MessagesController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        int userId = CurrentUserId;
        List<Conversation> conversations = await GetConversationsForCurrentUser();
        ViewBag.CurrentUserId = userId;
        ViewBag.UnreadCounts  = await GetUnreadCountsAsync(userId);
        return View(conversations);
    }

    public async Task<IActionResult> Open(int id)
    {
        int userId = CurrentUserId;

        Conversation? conversation = await _db.Conversations
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Id == id
                && c.Participants.Any(p => p.UserId == userId));

        if (conversation == null)
            return NotFound();

        foreach (Message msg in conversation.Messages.Where(m => m.SenderId != userId && !m.IsRead))
        {
            msg.IsRead = true;
            msg.ReadAt = DateTime.UtcNow;
        }

        ConversationParticipant? participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
            participant.LastReadAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        List<Conversation> conversations = await GetConversationsForCurrentUser();
        ViewBag.ActiveConversation = conversation;
        ViewBag.CurrentUserId = userId;
        ViewBag.UnreadCounts = await GetUnreadCountsAsync(userId);

        return View("Index", conversations);
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        int userId = CurrentUserId;

        var users = await _db.Users
            .Where(u => u.Id != userId && !u.IsAdmin && u.FullName.Contains(q))
            .Select(u => new { u.Id, u.FullName, u.Major })
            .Take(8)
            .ToListAsync();

        var result = users.Select(u =>
        {
            Major? majorEnum = EnumExtensions.ParseOrNull<Major>(u.Major);
            return new
            {
                u.Id,
                u.FullName,
                Major = majorEnum?.GetDisplayName() ?? u.Major ?? ""
            };
        });

        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartConversation(int targetUserId)
    {
        int userId = CurrentUserId;

        if (targetUserId == userId)
            return BadRequest();

        Conversation? existing = await _db.Conversations
            .Where(c => !c.IsGroup
                && c.Participants.Any(p => p.UserId == userId)
                && c.Participants.Any(p => p.UserId == targetUserId)
                && c.Participants.Count() == 2)
            .FirstOrDefaultAsync();

        if (existing != null)
            return RedirectToAction(nameof(Open), new { id = existing.Id });

        Conversation conversation = new Conversation
        {
            IsGroup   = false,
            CreatedAt = DateTime.UtcNow,
            Participants = new List<ConversationParticipant>
            {
                new ConversationParticipant { UserId = userId,       JoinedAt = DateTime.UtcNow },
                new ConversationParticipant { UserId = targetUserId, JoinedAt = DateTime.UtcNow }
            }
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Open), new { id = conversation.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return RedirectToAction(nameof(Open), new { id = conversationId });

        int userId = CurrentUserId;
        string actorName = User.Identity?.Name ?? "Someone";

        bool isParticipant = await _db.ConversationParticipants
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId);

        if (!isParticipant)
            return Forbid();

        List<int> recipientIds = await _db.ConversationParticipants
            .Where(p => p.ConversationId == conversationId && p.UserId != userId)
            .Select(p => p.UserId)
            .ToListAsync();

        _db.Messages.Add(new Message
        {
            ConversationId = conversationId,
            SenderId       = userId,
            Content        = content.Trim(),
            CreatedAt      = DateTime.UtcNow
        });

        await _db.Conversations
            .Where(c => c.Id == conversationId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LastMessageAt, DateTime.UtcNow));

        string trimmedContent = content.Trim();
        string preview = trimmedContent.Length > 80
            ? $"{trimmedContent[..77]}..."
            : trimmedContent;

        foreach (int recipientId in recipientIds)
        {
            _db.Notifications.Add(new Notification
            {
                Type = NotificationType.Message,
                Content = $"{actorName} sent you a message: {preview}",
                UserId = recipientId,
                ActorId = userId,
                LinkUrl = $"/Messages/Open/{conversationId}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Open), new { id = conversationId });
    }

    private async Task<List<Conversation>> GetConversationsForCurrentUser()
    {
        int userId = CurrentUserId;
        return await _db.Conversations
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();
    }

    private async Task<Dictionary<int, int>> GetUnreadCountsAsync(int userId)
    {
        return await _db.Messages
            .Where(m => m.Conversation.Participants.Any(p => p.UserId == userId)
                     && m.SenderId != userId
                     && !m.IsRead)
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConvId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConvId, x => x.Count);
    }
}