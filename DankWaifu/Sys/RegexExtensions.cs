using System;
using System.Text.RegularExpressions;

namespace DankWaifu.Sys
{
    public static class RegexExtensions
    {
        private const string ClassPath = "DankWaifu.Sys.RegexExtensions";

        /// <summary>
        /// Gets the specified group from the regex
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="input"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TryGetGroup(this Regex regex, string input, out string value, int index = 1)
        {
            value = string.Empty;

            if (regex == null)
                throw new ArgumentNullException($"regex was null at {ClassPath}.TryGetGroup");

            var match = regex.Match(input);
            if (match.Success)
                value = match.Groups[index].Value;

            return !string.IsNullOrEmpty(value);
        }
    }
}