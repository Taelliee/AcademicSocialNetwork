using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
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
        DateTime now = DateTime.UtcNow;
        List<Event> events = await _db.Events
            .Where(e => e.StartTime > now)
            .OrderBy(e => e.StartTime)
            .Take(3)
            .ToListAsync();

        return View(events);
    }
}