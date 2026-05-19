using System.Security.Claims;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Helpers;
using AcademicSocialNetwork.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (user.IsDeleted)
        {
            ModelState.AddModelError(string.Empty, "This account is no longer available.");
            return View(model);
        }

        user.IsOnline = true;
        user.LastActive = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await SignInAsync(user, model.RememberMe);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        var user = new Models.User
        {
            FullName     = model.FullName,
            Email        = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Major        = model.Major?.ToString(),   // stores "ComputerScience" etc.
            ClassYear    = model.ClassYear,
            CreatedAt    = DateTime.UtcNow,
            IsOnline     = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        if (userId > 0)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline   = false;
                user.LastActive = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private async Task SignInAsync(Models.User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new("ProfileImageUrl", user.ProfileImageUrl ?? "")
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(isPersistent ? 30 : 1)
            });
    }
}