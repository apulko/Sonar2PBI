namespace Sonar2PBI
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }
        public static bool HasValidInput(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }
    }
}
