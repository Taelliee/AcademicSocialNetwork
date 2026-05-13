using AcademicSocialNetwork.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/Account/Login";
        options.LogoutPath        = "/Account/Logout";
        options.AccessDeniedPath  = "/Account/Login";
        options.ExpireTimeSpan    = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        db.Database.Migrate();

        // Replaces placeholder "hashed" passwords with real hashes
        var seedUsers = db.Users.Where(u => u.PasswordHash == "hashed").ToList();
        if (seedUsers.Count > 0)
        {
            foreach (var u in seedUsers)
                u.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

            db.SaveChanges();
            logger.LogInformation("Seeded passwords for {Count} user(s). Default: 123456", seedUsers.Count);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup seeding failed.");
        throw;
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//Темата 15: Опростен модел на Академична социална мрежа.
//Изискванията: да има профили, публикации и връзки между потребители.