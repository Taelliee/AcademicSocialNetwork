namespace AcademicSocialNetwork.Helpers;

public static class AvatarHelper
{
    public static string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "??";

        string[] names = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (names.Length == 0)
            return "??";

        if (names.Length == 1)
            return names[0].Substring(0, Math.Min(2, names[0].Length)).ToUpper();

        return $"{names[0][0]}{names[^1][0]}".ToUpper();
    }
}