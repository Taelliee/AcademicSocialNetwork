using AcademicSocialNetwork.Controllers;
using AcademicSocialNetwork.Data;
using AcademicSocialNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AcademicSocialNetwork.Tests.Controllers;

public class HomeControllerTests
{
    private static AppDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        return new AppDbContext(options);
    }

    private static HomeController CreateController(AppDbContext db)
    {
        var logger = new Mock<ILogger<HomeController>>();
        return new HomeController(db, logger.Object);
    }

    [Fact]
    public async Task Index_ReturnsOnlyNonDeletedPosts_InDescendingCreatedAtOrder()
    {
        await using var db = CreateDb(nameof(Index_ReturnsOnlyNonDeletedPosts_InDescendingCreatedAtOrder));

        db.Users.AddRange(
            new User
            {
                Id = 1,
                FullName = "Alice Johnson",
                Email = "alice@example.com",
                PasswordHash = "hash1"
            },
            new User
            {
                Id = 2,
                FullName = "Bob Smith",
                Email = "bob@example.com",
                PasswordHash = "hash2"
            });

        db.Posts.AddRange(
            new Post
            {
                Id = 1,
                UserId = 1,
                Content = "Older visible post",
                CreatedAt = new DateTime(2026, 1, 1),
                IsDeleted = false
            },
            new Post
            {
                Id = 2,
                UserId = 2,
                Content = "Newest visible post",
                CreatedAt = new DateTime(2026, 2, 1),
                IsDeleted = false
            },
            new Post
            {
                Id = 3,
                UserId = 1,
                Content = "Deleted post",
                CreatedAt = new DateTime(2026, 3, 1),
                IsDeleted = true
            });

        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var actionResult = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(actionResult);
        var model = Assert.IsAssignableFrom<List<Post>>(viewResult.Model);

        Assert.Equal(2, model.Count);
        Assert.Equal(2, model[0].Id);
        Assert.Equal(1, model[1].Id);
        Assert.DoesNotContain(model, p => p.IsDeleted);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        using var db = CreateDb(nameof(Privacy_ReturnsViewResult));
        var controller = CreateController(db);

        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }
}