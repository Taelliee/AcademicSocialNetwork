using AcademicSocialNetwork.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.ViewComponents;

public class UpcomingEventsViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public UpcomingEventsViewComponent(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var now = DateTime.UtcNow;
        var events = await _db.Events
            .Where(e => e.StartTime > now)
            .OrderBy(e => e.StartTime)
            .Take(3)
            .ToListAsync();

        return View(events);
    }
}