using System.Text.RegularExpressions;

namespace _240519P_AS_ASSN2.Security
{
    public static class InputSanitizer
    {
        public static bool ContainsMaliciousScript(params string[] inputs)
        {
            var pattern = @"<\s*script\b|javascript:|onerror=|onload=";

            foreach (var input in inputs)
            {
                if (!string.IsNullOrEmpty(input) &&
                    Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
