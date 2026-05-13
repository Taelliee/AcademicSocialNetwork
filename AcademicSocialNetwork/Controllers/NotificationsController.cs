using System.Security.Claims;
using AcademicSocialNetwork.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var userId = CurrentUserId;

        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var unread = notifications.Where(n => !n.IsRead).ToList();
        unread.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
        await _db.SaveChangesAsync();

        return View(notifications);
    }
}