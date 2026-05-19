using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcademicSocialNetwork.ViewComponents;

public class ProfileCardViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public ProfileCardViewComponent(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int userId = int.TryParse(
            HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

        if (userId == 0)
            return Content(string.Empty);

        ProfileCardModel? user = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new ProfileCardModel
            {
                FullName = u.FullName,
                Major = u.Major,
                ClassYear = u.ClassYear,
                ProfileImageUrl = u.ProfileImageUrl,
                PostsCount = u.Posts.Count(p => !p.IsDeleted),
                ConnectionsCount = u.Followers.Count(c => c.Status == ConnectionStatus.Accepted)
                                  + u.Following.Count(c => c.Status == ConnectionStatus.Accepted)
            })
            .FirstOrDefaultAsync();

        return View(user);
    }
}

public class ProfileCardModel
{
    public string FullName { get; set; } = string.Empty;
    public string? Major { get; set; }
    public uint? ClassYear { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int PostsCount { get; set; }
    public int ConnectionsCount { get; set; }
}