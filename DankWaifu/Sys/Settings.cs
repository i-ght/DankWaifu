using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DankWaifu.Sys
{
    public static class Settings
    {
        private const string SettingTemplate = "{0}={1}";
        private static readonly string SettingFile;
        private static readonly Regex SettingRegex;
        private static readonly Dictionary<string, object> SettingsDict;

        static Settings()
        {
            SettingFile = $"{Assembly.GetEntryAssembly().GetName().Name}-settings.ini";
            SettingRegex = new Regex(".*?=.*?");
            SettingsDict = new Dictionary<string, object>();
        }

        /// <summary>
        /// Load the .ini file into the SettingsDict
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(SettingFile))
                using (new StreamWriter(SettingFile))
                {
                }

            lock (Lock)
            {
                using (var sw = new StreamReader(SettingFile))
                {
                    string line;
                    while ((line = sw.ReadLine()) != null)
                    {
                        if (!SettingRegex.IsMatch(line))
                            continue;

                        if (!SettingsDict.ContainsKey(line.Split('=')[0]))
                            SettingsDict.Add(line.Split('=')[0], line.Split('=')[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the setting of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(string name)
        {
            if (!SettingsDict.ContainsKey(name))
                return default(T);

            if (!typeof(T).IsEnum)
                return (T)Convert.ChangeType(SettingsDict[name], typeof(T));

            try
            {
                var result = (T)Enum.Parse(typeof(T), SettingsDict[name].ToString());
                return result;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Checks if the SettingsDict contains the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            return SettingsDict.ContainsKey(key);
        }

        private static readonly object Lock = new object();

        /// <summary>
        /// Saves the setting kvp to file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Save(string name, object value)
        {
            if (!SettingsDict.ContainsKey(name))
                SettingsDict.Add(name, value);
            else
                SettingsDict[name] = value;

            lock (Lock)
                using (var sw = new StreamWriter(SettingFile))
                    foreach (var setting in SettingsDict.Keys)
                        sw.WriteLine(SettingTemplate, setting, SettingsDict[setting]);
        }

        /// <summary>
        /// Selects a random value between setting1 and setting2
        /// </summary>
        /// <param name="minSetting"></param>
        /// <param name="maxSetting"></param>
        /// <returns></returns>
        public static int GetRandom(string minSetting, string maxSetting)
        {
            if (!SettingsDict.ContainsKey(minSetting) ||
                !SettingsDict.ContainsKey(maxSetting))
                return RandomHelpers.RandomInt(8, 13);

            return RandomHelpers.RandomInt(Get<int>(minSetting), Get<int>(maxSetting));
        }

        public static int GetRandom(string setting)
        {
            var minSetting = $"Min{setting}";
            var maxSetting = $"Max{setting}";

            return GetRandom(minSetting, maxSetting);
        }

        public static int SafeGetSettingInt(string minSetting, string maxSetting, int overrideMin = 3, int overrideMax = 9)
        {
            var min = Get<int>(minSetting);
            var max = Get<int>(maxSetting);

            if (min > max)
            {
                min = overrideMin;
                max = overrideMax;
            }

            return RandomHelpers.RandomInt(min, max);
        }
    }
}