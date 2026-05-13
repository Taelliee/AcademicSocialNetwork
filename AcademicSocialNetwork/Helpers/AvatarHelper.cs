namespace AcademicSocialNetwork.Helpers
{
    public static class AvatarHelper
    {
        public static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "??";

            var names = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (names.Length == 0)
                return "??";

            if (names.Length == 1)
                return names[0].Substring(0, Math.Min(2, names[0].Length)).ToUpper();

            // First and last name initials
            return $"{names[0][0]}{names[^1][0]}".ToUpper();
        }

        public static string GetAvatarColorClass(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "avatar-color-1";

            // Generate consistent color based on name hash
            int hash = fullName.GetHashCode();
            int colorIndex = Math.Abs(hash % 6) + 1;

            return $"avatar-color-{colorIndex}";
        }
    }
}