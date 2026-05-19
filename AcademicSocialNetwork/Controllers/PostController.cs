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
            Post post = new Post
            {
                Content = content,
                UserId = CurrentUserId,
                CreatedAt = DateTime.UtcNow,
                LinkUrl = string.IsNullOrWhiteSpace(linkUrl) ? null : linkUrl.Trim()
            };

            if (photo != null && photo.Length > 0)
            {
                string[] allowed = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
                string ext = Path.GetExtension(photo.FileName).ToLowerInvariant();

                if (allowed.Contains(ext))
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "posts");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = $"{Guid.NewGuid()}{ext}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using FileStream stream = new FileStream(filePath, FileMode.Create);
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
        Post? post = await _db.Posts.FindAsync(id);

        if (post != null && (post.UserId == CurrentUserId || User.IsInRole("Admin")))
        {
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                string filePath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/'));
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
    public async Task<IActionResult> LikeAjax(int postId)
    {
        int userId = CurrentUserId;
        string actorName = User.Identity?.Name ?? "Someone";

        Post? post = await _db.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            return NotFound();

        Like? existing = await _db.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        bool isLiked;

        if (existing == null)
        {
            _db.Likes.Add(new Like { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
            isLiked = true;
        }
        else
        {
            existing.IsDeleted = !existing.IsDeleted;
            isLiked = !existing.IsDeleted;
        }

        if (isLiked && post.UserId != userId)
        {
            _db.Notifications.Add(new Notification
            {
                Type = NotificationType.Like,
                Content = $"{actorName} liked your post.",
                UserId = post.UserId,
                ActorId = userId,
                PostId = post.Id,
                LinkUrl = "/",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        int likeCount = await _db.Likes.CountAsync(l => l.PostId == postId && !l.IsDeleted);
        return Json(new { liked = isLiked, likeCount });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CommentAjax(int postId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return BadRequest();

        string actorName = User.Identity?.Name ?? "Someone";
        int    userId    = CurrentUserId;

        Post? post = await _db.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            return NotFound();

        Comment comment = new Comment
        {
            Content = content,
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);

        if (post.UserId != userId)
        {
            _db.Notifications.Add(new Notification
            {
                Type = NotificationType.Comment,
                Content = $"{actorName} commented on your post.",
                UserId = post.UserId,
                ActorId = userId,
                PostId = post.Id,
                LinkUrl = "/",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        int commentCount = await _db.Comments.CountAsync(c => c.PostId == postId);
        return Json(new
        {
            commentCount,
            authorName = actorName,
            content = comment.Content,
            createdAt = comment.CreatedAt.ToString("MMM dd, HH:mm")
        });
    }
}