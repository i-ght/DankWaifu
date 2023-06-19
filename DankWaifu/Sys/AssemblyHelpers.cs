using System.Reflection;

namespace DankWaifu.Sys
{
    public static class AssemblyHelpers
    {
        public static string ApplicationName(bool replaceWhitespace = false, bool lcase = false)
        {
            var ret = Assembly.GetEntryAssembly().GetName().Name;
            if (replaceWhitespace)
                ret = ret.Replace(" ", "_");

            if (lcase)
                ret = ret.ToLower();

            return ret;
        }

        public static string ApplicationVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}