using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcademicSocialNetwork.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly AppDbContext _db;

    public NotificationsController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        int userId = CurrentUserId;

        List<Notification> notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        List<Notification> unread = notifications.Where(n => !n.IsRead).ToList();
        unread.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
        await _db.SaveChangesAsync();

        return View(notifications);
    }
}