using DankWaifu.Collections;

namespace DankWaifu.Sys
{
    public static class EmailHelpers
    {
        private static readonly string[] EmailDomains;

        static EmailHelpers()
        {
            EmailDomains = new[]
            {
                "aol.com",
                "yahoo.com",
                "outlook.com",
                "hotmail.com",
                "gmail.com",
                "yandex.com"
            };
        }

        public static string RandomEmailDomain()
        {
            return EmailDomains.RandomSelection();
        }
    }
}