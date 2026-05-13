using AcademicSocialNetwork.Models;

namespace AcademicSocialNetwork.ViewModels;

public class SearchResultsViewModel
{
    public string? Query { get; set; }
    public List<User> Users { get; set; } = new();
    public List<Post> Posts { get; set; } = new();
}