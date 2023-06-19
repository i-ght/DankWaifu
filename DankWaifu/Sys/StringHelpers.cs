using DankWaifu.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DankWaifu.Sys
{
    public enum StringDefinition
    {
        Digits,
        LowerLetters,
        UpperLetters,
        DigitsAndLowerLetters,
        DigitsAndUpperLetters,
        LowerAndUpperLetters,
        DigitsAndLowerUpperLetters,
        DigitsLowerUpperLettersUnderscoreDash,
        NonAlphaNumerical
    }

    public static class StringHelpers
    {
        private const string ClassPath = "DankWaifu.Text.StringHelpers";

        public static string RandomStringUniqueChars(int length, StringDefinition def)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), $"length must be > 0 at {ClassPath}.RandomString");

            Queue<char> chars;
            switch (def)
            {
                case StringDefinition.Digits:
                    chars = new Queue<char>(new List<char>("0123456789".ToCharArray()));
                    break;

                case StringDefinition.LowerLetters:
                    chars = new Queue<char>(new List<char>("abcdefghijklmnoqrstuvwxyz".ToCharArray()));
                    break;

                case StringDefinition.UpperLetters:
                    chars = new Queue<char>(new List<char>("ABCDEFGHIJKLMNOPWRSTUVWXYZ".ToCharArray()));
                    break;

                case StringDefinition.DigitsAndLowerLetters:
                    chars = new Queue<char>(new List<char>("0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray()));
                    break;

                case StringDefinition.DigitsAndUpperLetters:
                    chars = new Queue<char>(new List<char>("012345679ABCDEFGHIKLMNOPQRSTUVWXYZ".ToCharArray()));
                    break;

                case StringDefinition.LowerAndUpperLetters:
                    chars = new Queue<char>(new List<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()));
                    break;

                case StringDefinition.DigitsAndLowerUpperLetters:
                    chars = new Queue<char>(new List<char>("01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()));
                    break;

                case StringDefinition.NonAlphaNumerical:
                    chars = new Queue<char>(new List<char>("`~!@#$%^&*()-=_+[]\\;':\",./<>?"));
                    break;

                case StringDefinition.DigitsLowerUpperLettersUnderscoreDash:
                    chars = new Queue<char>(new List<char>("01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()));
                    break;

                default:
                    chars = new Queue<char>(new List<char>("abcdefghijklmnoqrstuvwxyz".ToCharArray()));
                    break;
            }

            if (length >= chars.Count)
                throw new ArgumentException("length must be less then or equal to the count of the chars array");

            chars.Shuffle();

            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
                sb.Append(chars.GetNext(false));

            chars.Clear();
            var ret = sb.ToString();
            return ret;
        }

        /// <summary>
        /// Returns a randomly generated string using the specified chars and length
        /// </summary>
        /// <param name="def"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length, StringDefinition def)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), $"length must be > 0 at {ClassPath}.RandomString");

            List<char> chars;
            switch (def)
            {
                case StringDefinition.Digits:
                    chars = new List<char>("0123456789".ToCharArray());
                    break;

                case StringDefinition.LowerLetters:
                    chars = new List<char>("abcdefghijklmnoqrstuvwxyz".ToCharArray());
                    break;

                case StringDefinition.UpperLetters:
                    chars = new List<char>("ABCDEFGHIJKLMNOPWRSTUVWXYZ".ToCharArray());
                    break;

                case StringDefinition.DigitsAndLowerLetters:
                    chars = new List<char>("0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray());
                    break;

                case StringDefinition.DigitsAndUpperLetters:
                    chars = new List<char>("012345679ABCDEFGHIKLMNOPQRSTUVWXYZ".ToCharArray());
                    break;

                case StringDefinition.LowerAndUpperLetters:
                    chars = new List<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
                    break;

                case StringDefinition.DigitsAndLowerUpperLetters:
                    chars = new List<char>("01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
                    break;

                case StringDefinition.NonAlphaNumerical:
                    chars = new List<char>("`~!@#$%^&*()-=_+[]\\;':\",./<>?");
                    break;

                case StringDefinition.DigitsLowerUpperLettersUnderscoreDash:
                    chars = new List<char>("01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-_".ToCharArray());
                    break;

                default:
                    chars = new List<char>("abcdefghijklmnoqrstuvwxyz".ToCharArray());
                    break;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
                sb.Append(chars[RandomHelpers.RandomInt(chars.Count)]);

            chars.Clear();
            var ret = sb.ToString();
            return ret;
        }

        /// <summary>
        /// Puts a space in between every occurence of a capital letter in a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CamelCasedToSpaced(string str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length <= 1)
                return null;

            var sb = new StringBuilder();
            sb.Append(char.ToUpper(str[0]));

            for (var i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]))
                    sb.Append(' ');
                sb.Append(str[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts the byte array to its hex string representation
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static string ByteArrayToHex(byte[] buf)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(buf), $"buf was null at {ClassPath}.ByteArrayToHex");

            if (buf.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var b in buf)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        /// <summary>
        /// Replaces {word1|word2} with a random selection
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Spin(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), $"input was null at {ClassPath}.Spin");

            return Regex.Replace(input, "{(.*?)}", delegate (Match match)
            {
                if (!match.Groups[1].Value.Contains("|"))
                    return match.Groups[1].Value;

                var splt = match.Groups[1].Value.Split('|');
                var ret = splt[RandomHelpers.RandomInt(splt.Length)];
                return ret;
            });
        }

        /// <summary>
        /// Reverse a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Replaces all new lines with string.Empty and single quotes with double quotes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Minify(string input)
        {
            var output = Regex.Replace(input, "\r\n?|\n", string.Empty).Replace("'", "\"");
            return output;
        }

        public static bool AllNullOrWhitespace(params string[] array)
        {
            if (array == null)
                return false;

            foreach (var str in array)
            {
                if (!string.IsNullOrWhiteSpace(str))
                    return false;
            }

            return true;
        }

        public static bool AnyNullOrEmpty(params string[] array)
        {
            foreach (var str in array)
                if (string.IsNullOrEmpty(str))
                    return true;

            return false;
        }

        public static string ReplaceNonAlphaNumerics(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public static string ReplaceNonAlphaNumericsAndNonWhiteSpace(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public static bool Contains(string input, params string[] array)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (array == null)
                throw new ArgumentNullException(nameof(array));

            foreach (var param in array)
            {
                if (!input.Contains(param))
                    return false;
            }

            return true;
        }

        public static bool AnyNullOrWhitespace(params string[] array)
        {
            foreach (var str in array)
                if (string.IsNullOrWhiteSpace(str))
                    return true;

            return false;
        }
    }
}