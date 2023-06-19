using System;
using System.Linq;

namespace DankWaifu.Sys
{
    public static class ByteArrayHelpers
    {
        private const string ClassPath = "DankWaifu.System.ByteArrayHelpers";

        /// <summary>
        /// Converts the hex string to a byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToBytes(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException(nameof(hex), $"hex was null at {ClassPath}.HexStringToBytes");

            if (string.IsNullOrEmpty(hex))
                return new byte[0];

            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}