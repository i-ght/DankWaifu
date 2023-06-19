using System;
using System.Collections.Generic;
using System.IO;

namespace DankWindowsWaifu.WPF
{
    public class SettingKeywords : SettingCollection
    {
        public SettingKeywords(string keyLabel, string keyValue)
        {
            KeyLabel = keyLabel;
            Key = keyValue;
            TypeLabel = "List";
            ValueObj = Activator.CreateInstance(typeof(Dictionary<string, List<string>>));

            TryLoadFromSettings();
        }

        public SettingKeywords(string key) : this(key, key)
        {
        }

        public override string Key { get; }
        public override string KeyLabel { get; }

        public override string TypeLabel { get; }
        public sealed override object ValueObj { get; protected set; }

        private Dictionary<string, List<string>> _value;

        public Dictionary<string, List<string>> Value
        {
            get
            {
                if (_value != null)
                    return _value;

                _value = (Dictionary<string, List<string>>)ValueObj;
                return _value;
            }
        }

        protected sealed override void TryLoadFromSettings()
        {
            if (!DankWaifu.Sys.Settings.ContainsKey(Key))
                return;

            var fileName = DankWaifu.Sys.Settings.Get<string>(Key);
            if (!File.Exists(fileName))
                return;

            if (!LoadKeywordsFromFile(fileName))
                return;

            FileName = fileName;
            ValueLabel = $"[{Value.Count:N0}] | {FileName}";
        }

        public bool LoadKeywordsFromFile(string fileName)
        {
            lock (Value)
            {
                Value.Clear();
                using (var sr = new StreamReader(fileName))
                {
                    string line;
                    while (!string.IsNullOrWhiteSpace(line = sr.ReadLine()))
                    {
                        line = line.ToLower();
                        var split = line.Split('|');
                        if (split.Length < 2)
                            continue;

                        var keyword = split[0];
                        if (string.IsNullOrWhiteSpace(keyword) || Value.ContainsKey(keyword))
                            continue;

                        var responses = new List<string>();
                        for (var i = 1; i < split.Length; i++)
                            responses.Add(split[i]);

                        Value.Add(keyword, responses);
                    }
                }
            }

            return Value.Count > 0;
        }
    }
}