using System.ComponentModel.DataAnnotations;
using AcademicSocialNetwork.Models;

namespace AcademicSocialNetwork.ViewModels;

public class RegisterViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public Major? Major { get; set; }

    [Range(1900, 2100)]
    [Display(Name = "Class Year")]
    public uint? ClassYear { get; set; }
}