using System.Security.Claims;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using AcademicSocialNetwork.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.Controllers;

[Authorize]
public class ConnectionsController : Controller
{
    private readonly AppDbContext _db;

    public ConnectionsController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /Connections
    public async Task<IActionResult> Index()
    {
        var userId = CurrentUserId;

        var all = await _db.Connections
            .Where(c => c.FollowerId == userId || c.FollowingId == userId)
            .Include(c => c.Follower)
            .Include(c => c.Following)
            .ToListAsync();

        var vm = new ConnectionsViewModel
        {
            CurrentUserId = userId,
            Accepted      = all.Where(c => c.Status == ConnectionStatus.Accepted).ToList(),
            Incoming      = all.Where(c => c.Status == ConnectionStatus.Pending && c.FollowingId == userId).ToList(),
            Outgoing      = all.Where(c => c.Status == ConnectionStatus.Pending && c.FollowerId  == userId).ToList()
        };

        return View(vm);
    }

    // GET /Connections/Discover
    public async Task<IActionResult> Discover()
    {
        var userId = CurrentUserId;

        var connectedIds = await _db.Connections
            .Where(c => c.FollowerId == userId || c.FollowingId == userId)
            .Select(c => c.FollowerId == userId ? c.FollowingId : c.FollowerId)
            .ToListAsync();

        connectedIds.Add(userId);

        var users = await _db.Users
            .Where(u => !connectedIds.Contains(u.Id) && !u.IsAdmin)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(users);
    }

    // POST /Connections/Connect
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Connect(int targetUserId, string? returnUrl = null)
    {
        var userId = CurrentUserId;

        var exists = await _db.Connections
            .AnyAsync(c => c.FollowerId == userId && c.FollowingId == targetUserId);

        if (!exists)
        {
            _db.Connections.Add(new Connection
            {
                FollowerId  = userId,
                FollowingId = targetUserId,
                Status      = ConnectionStatus.Pending,
                CreatedAt   = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Discover));
    }

    // POST /Connections/Accept
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int connectionId)
    {
        var connection = await _db.Connections.FindAsync(connectionId);

        if (connection != null && connection.FollowingId == CurrentUserId)
        {
            connection.Status     = ConnectionStatus.Accepted;
            connection.AcceptedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Connections/Reject
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int connectionId)
    {
        var connection = await _db.Connections.FindAsync(connectionId);

        if (connection != null && connection.FollowingId == CurrentUserId)
        {
            _db.Connections.Remove(connection);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Connections/Cancel  (withdraw a sent request)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int connectionId)
    {
        var connection = await _db.Connections.FindAsync(connectionId);

        if (connection != null && connection.FollowerId == CurrentUserId
                               && connection.Status == ConnectionStatus.Pending)
        {
            _db.Connections.Remove(connection);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Connections/Remove  (unfriend an accepted connection)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int connectionId)
    {
        var userId     = CurrentUserId;
        var connection = await _db.Connections.FindAsync(connectionId);

        if (connection != null
            && connection.Status == ConnectionStatus.Accepted
            && (connection.FollowerId == userId || connection.FollowingId == userId))
        {
            _db.Connections.Remove(connection);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}