using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DankWaifu.Net.Imap
{
    internal static class ImapHelpers
    {
        private static readonly Regex DecodeWordRegex;

        static ImapHelpers()
        {
            DecodeWordRegex = new Regex(
                @"=\?([A-Za-z0-9\-_]+)(?:\*[^\?]+)?\?([BbQq])\?([^\?]+)\?=",
                RegexOptions.Compiled
            );
        }

        /// <summary>
        /// Decodes a string composed of one or several MIME 'encoded-words'.
        /// </summary>
        /// <param name="words">A string to composed of one or several MIME 'encoded-words'.</param>
        /// <exception cref="FormatException">An unknown encoding (other than Q-Encoding or Base64) is
        /// encountered.</exception>
        /// <returns>A concatenation of all enconded-words in the passed string</returns>
        internal static string DecodeWords(string words)
        {
            if (string.IsNullOrEmpty(words))
                return string.Empty;

            var matches = DecodeWordRegex.Matches(words);
            if (matches.Count == 0)
                return words;
            // http://tools.ietf.org/html/rfc2047#page-10:
            // When displaying a particular header field that contains multiple 'encoded-word's, any
            // 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored. (This is
            // to allow the use of multiple 'encoded-word's to represent long strings of unencoded text,
            // without having to separate 'encoded-word's where spaces occur in the unencoded text.)
            // line-white-space ref: http://tools.ietf.org/html/rfc2616#page-16
            var decoded = new StringBuilder();
            // Keep track of and use separation data between 'encoded-word's.
            var lastKnownMatchPos = 0;
            foreach (Match m in matches)
            {
                if (m.Index > lastKnownMatchPos)
                    HandleFillData(decoded, words.Substring(lastKnownMatchPos, m.Index -
                        lastKnownMatchPos));
                decoded.Append(DecodeWord(m.Groups[0].Value));
                lastKnownMatchPos = m.Index + m.Length;
            }
            HandleFillData(decoded, words.Substring(lastKnownMatchPos));
            return decoded.ToString();
        }

        /// <summary>
        /// Internal function reuse to add separation between multiple 'encoded-word's correctly.
        /// </summary>
        private static void HandleFillData(StringBuilder decoded, string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            // Cr or Lf is never in the result.
            var fillData = data.Replace("\r", "").Replace("\n", "");
            // Any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored.
            if (fillData.Trim().Length != 0)
                decoded.Append(fillData);
        }

        /// <summary>
        /// Decodes a MIME 'encoded-word' string.
        /// </summary>
        /// <param name="word">The encoded word to decode</param>
        /// <exception cref="FormatException">An unknown encoding (other than Q-Encoding or Base64) is
        /// encountered.</exception>
        /// <returns>A decoded string</returns>
        /// <remarks>MIME encoded-word syntax is a way to encode strings that contain non-ASCII data.
        /// Commonly used encodings for the encoded-word sytax are Q-Encoding and Base64. For an
        /// in-depth description, refer to RFC 2047.</remarks>
        private static string DecodeWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            var m = DecodeWordRegex.Match(word);
            if (!m.Success)
                return word;
            var encoding = GetEncoding(m.Groups[1].Value);
            var type = m.Groups[2].Value.ToUpper();
            var text = m.Groups[3].Value;

            switch (type)
            {
                case "Q":
                    return QDecode(text, encoding);

                case "B":
                    return encoding.GetString(Convert.FromBase64String(text));

                default:
                    throw new FormatException("Encoding not recognized in encoded word: " + word);
            }
        }

        private static string QDecode(string value, Encoding encoding)
        {
            try
            {
                using (var m = new MemoryStream())
                {
                    for (var i = 0; i < value.Length; i++)
                    {
                        switch (value[i])
                        {
                            case '=':
                                var hex = value.Substring(i + 1, 2);
                                m.WriteByte(Convert.ToByte(hex, 16));
                                i = i + 2;
                                break;

                            case '_':
                                m.WriteByte(Convert.ToByte(' '));
                                break;

                            default:
                                m.WriteByte(Convert.ToByte(value[i]));
                                break;
                        }
                    }
                    return encoding.GetString(m.ToArray());
                }
            }
            catch
            {
                throw new FormatException("value is not a valid Q-encoded string.");
            }
        }

        private static Encoding GetEncoding(string name)
        {
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(name);
            }
            catch
            {
                encoding = Encoding.ASCII;
            }

            return encoding;
        }
    }
}