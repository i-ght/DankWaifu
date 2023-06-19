using DankWaifu.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace DankWindowsWaifu.WPF
{
    public class SettingList : SettingCollection
    {
        public sealed override string Key { get; }
        public override string KeyLabel { get; }
        public override string TypeLabel { get; }
        public sealed override object ValueObj { get; protected set; }

        private List<string> _value;

        public List<string> Value
        {
            get
            {
                if (_value != null)
                    return _value;

                _value = (List<string>)ValueObj;
                return _value;
            }
        }

        public SettingList(string keyLabel, string keyValue)
        {
            KeyLabel = keyLabel;
            Key = keyValue;
            TypeLabel = "List";
            ValueObj = Activator.CreateInstance(typeof(List<string>));

            TryLoadFromSettings();
        }

        public SettingList(string key) : this(key, key)
        {
        }

        /// <summary>
        /// Tries to load the saved setting from file if it exists
        /// </summary>
        protected sealed override void TryLoadFromSettings()
        {
            if (!DankWaifu.Sys.Settings.ContainsKey(Key))
                return;

            var fileName = DankWaifu.Sys.Settings.Get<string>(Key);
            if (!File.Exists(fileName))
                return;

            FileName = fileName;
            Value.LoadFromFile(fileName);
            ValueLabel = $"[{Value.Count:N0}] | {fileName}";
        }
    }
}