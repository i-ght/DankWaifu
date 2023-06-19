using System;
using System.Text;

namespace DankWaifu.Crypto
{
    public static class CryptoHelpers
    {
        private const string ClassPath = "DankWaifu.Crypto.CryptoHelpers";

        public static byte[] MD5(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), $"input was null at {ClassPath}.MD5");

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var ret = md5.ComputeHash(input);
                return ret;
            }
        }

        public static byte[] MD5(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), $"input was null at {ClassPath}.MD5");

            var ret = MD5(Encoding.UTF8.GetBytes(input));
            return ret;
        }

        public static string MD5Hex(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), $"input was null at {ClassPath}.MD5Hex");

            var hash = MD5(input);
            var ret = ByteArrayToHex(hash);
            return ret;
        }

        public static string MD5Hex(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), $"input was null at {ClassPath}.MD5Hex");

            var ret = MD5Hex(Encoding.UTF8.GetBytes(input));
            return ret;
        }

        public static string MD5Base64(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), $"input was null at {ClassPath}.MD5Base64");

            var hash = MD5(input);
            var ret = Convert.ToBase64String(hash);
            return ret;
        }

        public static string MD5Base64(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), $"input was null at {ClassPath}.MD5Base64");

            var ret = MD5Base64(Encoding.UTF8.GetBytes(input));
            return ret;
        }

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
    }
}