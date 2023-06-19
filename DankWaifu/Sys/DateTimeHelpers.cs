using System;

namespace DankWaifu.Sys
{
    public static class DateTimeHelpers
    {
        private static readonly DateTime Epoch;
        private static readonly string[] UsTimeZoneIds;

        static DateTimeHelpers()
        {
            Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            UsTimeZoneIds = new[]
            {
                "Pacific Standard Time",
                "Mountain Standard Time",
                "Central Standard Time",
                "Eastern Standard Time"
            };

            PacificTime = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            MountainTime = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
            CentralTime = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            EasternTime = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        public static TimeZoneInfo PacificTime { get; }
        public static TimeZoneInfo MountainTime { get; }
        public static TimeZoneInfo CentralTime { get; }
        public static TimeZoneInfo EasternTime { get; }

        /// <summary>
        /// Returns a randomly selected timezone id
        /// </summary>
        /// <returns></returns>
        public static string RandomTimeZoneId()
        {
            return UsTimeZoneIds[RandomHelpers.RandomInt(UsTimeZoneIds.Length)];
        }

        /// <summary>
        /// Returns a randonly selected timezone info
        /// </summary>
        /// <returns></returns>
        public static TimeZoneInfo RandomUsTimeZone()
        {
            var timezoneId = RandomTimeZoneId();
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return tzInfo;
        }

        /// <summary>
        /// Returns utc offset in seconds of specified timezone id
        /// </summary>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        public static int UtcOffsetInSeconds(string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var now = DateTime.UtcNow;
            var nowInTimezone = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Utc, tz);
            var offset = tz.GetUtcOffset(nowInTimezone);
            return (int)offset.TotalSeconds;
        }

        /// <summary>
        /// Returns utc offset in seconds of specified timezone id
        /// </summary>
        /// <param name="tz"></param>
        /// <returns></returns>
        public static int UtcOffsetInSeconds(TimeZoneInfo tz)
        {
            var now = DateTime.UtcNow;
            var nowInTimezone = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Utc, tz);
            var offset = tz.GetUtcOffset(nowInTimezone);
            return (int)offset.TotalSeconds;
        }

        /// <summary>
        /// Gets the current UTC timestamp with the specified length
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long UtcTimestamp(DateTime dt, int length)
        {
            var ret = dt.ToUniversalTime().Ticks - Epoch.Ticks;
            return long.Parse($"{ret.ToString().Substring(0, length)}");
        }

        /// <summary>
        /// Gets the now of the timezone info
        /// </summary>
        /// <param name="tzInfo"></param>
        /// <returns></returns>
        public static DateTime NowInTimeZone(TimeZoneInfo tzInfo)
        {
            var now = DateTime.Now;
            var nowInTimeZone = TimeZoneInfo.ConvertTime(now, tzInfo);
            return nowInTimeZone;
        }

        /// <summary>
        /// Gets the current UTC timestamps total milliseconds
        /// </summary>
        /// <returns></returns>
        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
        }

        /// <summary>
        /// Returns utc offset in minutes of specified timzone id
        /// </summary>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        public static int UtcOffsetInMinutes(string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var now = DateTime.UtcNow;
            var nowInTimezone = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Utc, tz);
            var offset = tz.GetUtcOffset(nowInTimezone);
            return (int)offset.TotalMinutes;
        }

        /// <summary>
        /// Returns utc offset in minutes of specified timzone id
        /// </summary>
        /// <returns></returns>
        public static int UtcOffsetInMinutes(TimeZoneInfo tz)
        {
            var now = DateTime.UtcNow;
            var nowInTimezone = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Utc, tz);
            var offset = tz.GetUtcOffset(nowInTimezone);
            return (int)offset.TotalMinutes;
        }

        /// <summary>
        /// Gets the current UTC timestamp with the specified length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long UtcTimeStamp(int length)
        {
            return long.Parse($"{UtcTimeStamp().ToString().Substring(0, length)}");
        }

        /// <summary>
        /// Gets the current UTC timestamp
        /// </summary>
        /// <returns></returns>
        public static long UtcTimeStamp()
        {
            var dt = DateTime.UtcNow.ToUniversalTime();
            var ret = dt.ToUniversalTime().Ticks - Epoch.Ticks;
            return ret;
        }

        /// <summary>
        /// Generates a random date of birth
        /// </summary>
        /// <param name="minAge"></param>
        /// <param name="maxAge"></param>
        /// <returns></returns>
        public static DateTime GenerateDateOfBirth(int minAge, int maxAge)
        {
            int thisYear = DateTime.Now.Year,
                birthYear = RandomHelpers.RandomInt(minAge, maxAge),
                month = RandomHelpers.RandomInt(1, 12),
                day = RandomHelpers.RandomInt(1, 27);

            var ret = new DateTime(thisYear - birthYear, month, day);
            if (ret.AddYears(18) >= DateTime.Now)
                ret = ret.AddYears(-1);

            return ret;
        }

        public static string TimezoneAbreviation(string timezoneId)
        {
            switch (timezoneId)
            {
                case "Pacific Standard Time":
                    return "PST";

                case "Mountain Standard Time":
                    return "MST";

                case "Eastern Standard Time":
                    return "EST";

                default:
                    return "CST";
            }
        }
    }
}