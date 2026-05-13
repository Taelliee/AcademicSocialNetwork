using System.Security.Claims;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Helpers;
using AcademicSocialNetwork.Models;
using AcademicSocialNetwork.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return View(new SearchResultsViewModel { Query = q });

        var term = q.Trim();

        var users = await _db.Users
            .Where(u => !u.IsAdmin && u.Id != CurrentUserId && u.FullName.Contains(term))
            .ToListAsync();

        var posts = await _db.Posts
            .Include(p => p.Author)
            .Include(p => p.Likes)
            .Include(p => p.Comments.Where(c => !c.IsDeleted))
            .Where(p => !p.IsDeleted && p.Content.Contains(term))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var vm = new SearchResultsViewModel
        {
            Query   = term,
            Users   = users,
            Posts   = posts
        };

        return View(vm);
    }
}