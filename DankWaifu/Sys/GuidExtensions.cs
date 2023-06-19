using System;

namespace DankWaifu.Sys
{
    public static class GUIDExtensions
    {
        public static long MostSignificantBits(this Guid guid)
        {
            var data = guid.ToByteArray();
            long msb = 0;
            for (var i = 0; i < 8; i++)
                msb = msb << 8 | (data[i] & 0xff);
            return msb;
        }

        public static long LeastSignificantBits(this Guid guid)
        {
            var data = guid.ToByteArray();
            long lsb = 0;
            for (var i = 8; i < 16; i++)
                lsb = (lsb << 8) | (data[i] & 0xff);
            return lsb;
        }
    }
}