using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AcademicSocialNetwork.Helpers;

public static class EnumExtensions
{
    /// <summary>Returns the [Display(Name = "...")] value, or the member name if no attribute.</summary>
    public static string GetDisplayName(this Enum value)
    {
        var member  = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? value.ToString();
    }

    /// <summary>Returns the [Description("...")] abbreviation, or the display name if no attribute.</summary>
    public static string GetAbbreviation(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var desc   = member?.GetCustomAttribute<DescriptionAttribute>();
        return desc?.Description ?? value.GetDisplayName();
    }

    /// <summary>Parses a stored enum member name back to the enum. Returns null if unrecognised.</summary>
    public static T? ParseOrNull<T>(string? value) where T : struct, Enum =>
        Enum.TryParse<T>(value, out var result) ? result : null;
}