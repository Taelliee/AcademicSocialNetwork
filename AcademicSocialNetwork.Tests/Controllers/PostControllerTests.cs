using System.IO;
using System.Security.Claims;
using AcademicSocialNetwork.Controllers;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcademicSocialNetwork.Tests.Controllers;

public class PostControllerTests
{
    private static AppDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        return new AppDbContext(options);
    }

    private static PostController CreateController(AppDbContext db, int userId, bool isAdmin = false)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var controller = new PostController(db, env.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateUser(userId, isAdmin)
            }
        };

        return controller;
    }

    private static ClaimsPrincipal CreateUser(int userId, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    // ── Test 1 ────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Delete_ByOwner_SoftDeletesPost()
    {
        await using var db = CreateDb(nameof(Delete_ByOwner_SoftDeletesPost));

        var post = new Post { Id = 1, UserId = 10, Content = "Hello" };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 10);
        await controller.Delete(1);

        var result = await db.Posts.FindAsync(1);
        Assert.True(result!.IsDeleted);
    }

    // ── Test 2 ────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Delete_ByNonOwner_DoesNotDeletePost()
    {
        await using var db = CreateDb(nameof(Delete_ByNonOwner_DoesNotDeletePost));

        var post = new Post { Id = 2, UserId = 99, Content = "Other user post" };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 10);
        await controller.Delete(2);

        var result = await db.Posts.FindAsync(2);
        Assert.False(result!.IsDeleted);
    }

    // ── Test 3 ────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Delete_ByAdmin_DeletesAnyPost()
    {
        await using var db = CreateDb(nameof(Delete_ByAdmin_DeletesAnyPost));

        var post = new Post { Id = 3, UserId = 50, Content = "Some user post" };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1, isAdmin: true);
        await controller.Delete(3);

        var result = await db.Posts.FindAsync(3);
        Assert.True(result!.IsDeleted);
    }
}