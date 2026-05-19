using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AcademicSocialNetwork.Helpers;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        MemberInfo? member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        DisplayAttribute? display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? value.ToString();
    }

    public static string GetAbbreviation(this Enum value)
    {
        MemberInfo? member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        DescriptionAttribute? desc = member?.GetCustomAttribute<DescriptionAttribute>();
        return desc?.Description ?? value.GetDisplayName();
    }

    public static T? ParseOrNull<T>(string? value) where T : struct, Enum =>
        Enum.TryParse<T>(value, out T result) ? result : null;
}