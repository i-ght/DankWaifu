using DankWaifu.Sys;
using System;
using System.Net;
using System.Text;

namespace DankWaifu.Net
{
    public static class NetHelpers
    {
        private const string ClassPath = "DankWaifu.Net.NetHelpers";

        private static readonly char[] HexChars;

        static NetHelpers()
        {
            HexChars = "ABCDEF0123456789".ToCharArray();
        }

        /// <summary>
        /// Generates a random MAC address
        /// </summary>
        /// <returns></returns>
        public static string RandomMacAddress()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 6; i++)
            {
                var temp = GenerateHexString(2).ToUpper();
                sb.Append(temp + ":");
            }

            var ret = sb.ToString().TrimEnd(':');
            return ret;
        }

        /// <summary>
        /// Generates a random hex string
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateHexString(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), $"length argument was <= 0 at {ClassPath}.GenerateHexString");

            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
                sb.Append(HexChars[RandomHelpers.RandomInt(HexChars.Length)]);

            return sb.ToString();
        }

        /// <summary>
        /// Converts a string to a WebProxy object
        /// </summary>
        /// <param name="str"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static bool TryParseProxy(string str, out WebProxy proxy)
        {
            proxy = null;

            try
            {
                if (string.IsNullOrWhiteSpace(str) || !str.Contains(":"))
                    return false;

                var split = str.Split(':');
                switch (split.Length)
                {
                    case 2:
                        proxy = new WebProxy(split[0], int.Parse(split[1]));
                        break;

                    case 4:
                        proxy = new WebProxy(split[0], int.Parse(split[1]))
                        {
                            Credentials = new NetworkCredential(split[2], split[3])
                        };
                        break;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}