namespace DankWaifu.Sys
{
    public static class StringExtensions
    {
        public static string JavaSubstring(this string s, int beginIndex,
            int endIndex)
        {
            // simulates Java substring function
            var len = endIndex - beginIndex;
            return s.Substring(beginIndex, len);
        }
    }
}