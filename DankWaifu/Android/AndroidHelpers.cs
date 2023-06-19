using DankWaifu.Crypto;
using DankWaifu.Net;
using DankWaifu.Sys;
using System;
using System.Collections.Generic;

namespace DankWaifu.Android
{
    public static class AndroidHelpers
    {
        private const string GcmPrefix = "APA91b";

        private static readonly char[] ValidFirstChars;
        private static readonly List<DpiInfo> AndroidDpis;

        static AndroidHelpers()
        {
            ValidFirstChars = new[] { 'a', 'b', 'c', 'd', 'e', 'f' };

            AndroidDpis = new List<DpiInfo>();
            var tmp = Enum.GetValues(typeof(DpiType));
            foreach (var val in tmp)
                AndroidDpis.Add(new DpiInfo((DpiType)val));
        }

        /// <summary>
        /// Returns a randomly generated GCM token.
        /// </summary>
        /// <returns></returns>
        public static string RandomGcmToken()
        {
            return RandomGcmToken(155);
        }

        /// <summary>
        /// Returns a randomly generated GCM token with the specified length
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string RandomGcmToken(int len)
        {
            return
                $"{GcmPrefix}{StringHelpers.RandomString(1, StringDefinition.UpperLetters)}" +
                $"{StringHelpers.RandomString(len, StringDefinition.DigitsLowerUpperLettersUnderscoreDash)}";
        }

        public static DpiInfo RandomDpi()
        {
            return AndroidDpis[RandomHelpers.RandomInt(AndroidDpis.Count)];
        }

        ///<summary>
        /// Generates a random android id
        /// </summary>
        /// <returns></returns>
        public static string RandomAndroidId()
        {
            return
                $"{ValidFirstChars[RandomHelpers.RandomInt(ValidFirstChars.Length)]}" +
                $"{CryptoHelpers.MD5Hex(Guid.NewGuid().ToString()).Substring(0, 15)}";
        }

        /// <summary>
        /// Returns a randomly generated android serial identifier.
        /// </summary>
        /// <returns></returns>
        public static string RandomAndroidSerial()
        {
            //C4F12FDD949F22F
            return NetHelpers.GenerateHexString(16);
        }
    }
}