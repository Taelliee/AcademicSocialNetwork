using System.Security.Claims;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.Controllers;

[Authorize]
public class PostController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PostController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string content, IFormFile? photo, string? linkUrl)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            var post = new Post
            {
                Content   = content,
                UserId    = CurrentUserId,
                CreatedAt = DateTime.UtcNow,
                LinkUrl   = string.IsNullOrWhiteSpace(linkUrl) ? null : linkUrl.Trim()
            };

            if (photo != null && photo.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext     = Path.GetExtension(photo.FileName).ToLowerInvariant();

                if (allowed.Contains(ext))
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "posts");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await photo.CopyToAsync(stream);

                    post.ImageUrl = $"/uploads/posts/{fileName}";
                }
            }

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _db.Posts.FindAsync(id);

        if (post != null && (post.UserId == CurrentUserId || User.IsInRole("Admin")))
        {
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            post.IsDeleted = true;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int postId)
    {
        var userId = CurrentUserId;
        var actorName = User.Identity?.Name ?? "Someone";

        var post = await _db.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            return RedirectToAction("Index", "Home");

        var existing = await _db.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        var shouldNotify = false;

        if (existing == null)
        {
            _db.Likes.Add(new Like
            {
                PostId    = postId,
                UserId    = userId,
                CreatedAt = DateTime.UtcNow
            });

            shouldNotify = true;
        }
        else
        {
            existing.IsDeleted = !existing.IsDeleted;
            shouldNotify = !existing.IsDeleted;
        }

        if (shouldNotify && post.UserId != userId)
        {
            _db.Notifications.Add(new Notification
            {
                Type      = NotificationType.Like,
                Content   = $"{actorName} liked your post.",
                UserId    = post.UserId,
                ActorId   = userId,
                PostId    = post.Id,
                LinkUrl   = "/",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(int postId, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            var actorName = User.Identity?.Name ?? "Someone";
            var post = await _db.Posts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return RedirectToAction("Index", "Home");

            _db.Comments.Add(new Comment
            {
                Content   = content,
                PostId    = postId,
                UserId    = CurrentUserId,
                CreatedAt = DateTime.UtcNow
            });

            if (post.UserId != CurrentUserId)
            {
                _db.Notifications.Add(new Notification
                {
                    Type      = NotificationType.Comment,
                    Content   = $"{actorName} commented on your post.",
                    UserId    = post.UserId,
                    ActorId   = CurrentUserId,
                    PostId    = post.Id,
                    LinkUrl   = "/",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }
}