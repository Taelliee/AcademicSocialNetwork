using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.ViewComponents;

public class TrendingTopicsViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public TrendingTopicsViewComponent(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        List<Tag> tags = await _db.Tags
            .Where(t => t.UseCount > 0)
            .OrderByDescending(t => t.UseCount)
            .Take(6)
            .ToListAsync();

        return View(tags);
    }
}