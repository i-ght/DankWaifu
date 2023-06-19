using System;

namespace DankWaifu.Sys
{
    public static class RandomHelpers
    {
        private static readonly Random Random;

        static RandomHelpers()
        {
            Random = new Random();
        }

        public static int RandomInt(int max)
        {
            lock (Random)
                return Random.Next(max);
        }

        public static int RandomInt(int min, int max)
        {
            lock (Random)
                return Random.Next(min, max);
        }

        public static void RandomBytes(byte[] buffer)
        {
            lock (Random)
                Random.NextBytes(buffer);
        }

        public static double RandomDouble()
        {
            lock (Random)
                return Random.NextDouble();
        }
    }
}