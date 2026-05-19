using System.Security.Claims;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.ViewComponents;

public class SuggestedConnectionsViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public SuggestedConnectionsViewComponent(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int userId = int.TryParse(
            HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

        if (userId == 0)
            return Content(string.Empty);

        List<int> connectedIds = await _db.Connections
            .Where(c => c.FollowerId == userId || c.FollowingId == userId)
            .Select(c => c.FollowerId == userId ? c.FollowingId : c.FollowerId)
            .ToListAsync();

        connectedIds.Add(userId);

        List<User> suggestions = await _db.Users
            .Where(u => !connectedIds.Contains(u.Id) && !u.IsAdmin)
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .ToListAsync();

        return View(suggestions);
    }
}