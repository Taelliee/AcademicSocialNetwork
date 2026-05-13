using System.ComponentModel.DataAnnotations;
using AcademicSocialNetwork.Models;

namespace AcademicSocialNetwork.ViewModels;

public class EditProfileViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    public Major? Major { get; set; }

    [Range(1900, 2100)]
    [Display(Name = "Class Year")]
    public uint? ClassYear { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Profile Photo")]
    public IFormFile? ProfileImage { get; set; }
}