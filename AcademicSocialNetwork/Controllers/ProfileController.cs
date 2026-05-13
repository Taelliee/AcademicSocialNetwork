using System.Security.Claims;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using AcademicSocialNetwork.ViewModels;
using AcademicSocialNetwork.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication; // Add this using directive at the top

namespace AcademicSocialNetwork.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileController(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /Profile      → logged-in user's own profile
    // GET /Profile?id=3 → another user's profile
    public async Task<IActionResult> Index(int? id)
    {
        var userId = id ?? CurrentUserId;

        var user = await _db.Users
            .Include(u => u.Posts.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Likes)
            .Include(u => u.Posts.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Comments.Where(c => !c.IsDeleted))
            .Include(u => u.Followers)
                .ThenInclude(c => c.Follower)
            .Include(u => u.Following)
                .ThenInclude(c => c.Following)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        ViewBag.IsOwnProfile = userId == CurrentUserId;
        return View(user);
    }

    // GET /Profile/Edit
    public async Task<IActionResult> Edit()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        var vm = new EditProfileViewModel
        {
            FullName        = user.FullName,
            Major           = EnumExtensions.ParseOrNull<Major>(user.Major),
            ClassYear       = user.ClassYear,
            Bio             = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl
        };

        return View(vm);
    }

    // POST /Profile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        user.FullName  = model.FullName;
        user.Major     = model.Major?.ToString();
        user.ClassYear = model.ClassYear;
        user.Bio       = model.Bio;

        if (model.ProfileImage is { Length: > 0 })
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext     = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(nameof(model.ProfileImage), "Only image files are allowed.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                var old = Path.Combine(_env.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
            }

            var folder   = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}{ext}";

            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await model.ProfileImage.CopyToAsync(stream);

            user.ProfileImageUrl = $"/uploads/profiles/{fileName}";
        }

        await _db.SaveChangesAsync();

        // Re-issue the auth cookie so the ProfileImageUrl claim reflects the new photo
        var claims = new List<System.Security.Claims.Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.FullName),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           user.IsAdmin ? "Admin" : "User"),
            new("ProfileImageUrl",         user.ProfileImageUrl ?? "")
        };
        var identity  = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction(nameof(Index));
    }
}