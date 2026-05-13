using System.Security.Claims;
using AcademicSocialNetwork.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Admin
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(users);
    }

    // POST /Admin/ToggleAdmin/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (id == currentUserId)
        {
            TempData["Error"] = "You cannot change your own admin status.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.IsAdmin = !user.IsAdmin;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{user.FullName} is now {(user.IsAdmin ? "an admin" : "a regular user")}.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/DeleteUser/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (id == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.IsOnline = false;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"{user.FullName}'s account has been deleted.";
        return RedirectToAction(nameof(Index));
    }
}