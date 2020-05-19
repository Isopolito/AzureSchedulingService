namespace Scheduling.SharedPackage.Extensions
{
    public static class StringExtensions
    {
        public static bool HasValue(this string text) => !string.IsNullOrEmpty(text);
        public static bool HasNoValue(this string text) => string.IsNullOrEmpty(text);
    }
}
