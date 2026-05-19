using System.Security.Claims;
using AcademicSocialNetwork.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.ViewComponents;

public class UnreadBadgesModel
{
    public int Notifications { get; set; }
    public int Messages      { get; set; }
}

public class UnreadBadgesViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public UnreadBadgesViewComponent(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int userId = int.TryParse(
            HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

        if (userId == 0)
            return View(new UnreadBadgesModel());

        int unreadNotifications = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        int unreadMessages = await _db.Messages
            .CountAsync(m => m.Conversation.Participants.Any(p => p.UserId == userId)
                          && m.SenderId != userId
                          && !m.IsRead);

        return View(new UnreadBadgesModel
        {
            Notifications = unreadNotifications,
            Messages      = unreadMessages
        });
    }
}