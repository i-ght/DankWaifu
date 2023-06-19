using DankWaifu.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace DankWindowsWaifu.WPF
{
    public sealed class SettingQueue : SettingCollection
    {
        private Queue<string> _value;

        public override string Key { get; }
        public override string KeyLabel { get; }
        public override string TypeLabel { get; }
        public override object ValueObj { get; protected set; }

        public Queue<string> Value
        {
            get
            {
                if (_value != null)
                    return _value;

                _value = (Queue<string>)ValueObj;
                return _value;
            }
        }

        public SettingQueue(string keyLabel, string keyValue)
        {
            KeyLabel = keyLabel;
            Key = keyValue;
            TypeLabel = "List";
            ValueObj = Activator.CreateInstance(typeof(Queue<string>));

            TryLoadFromSettings();
        }

        public SettingQueue(string key) : this(key, key)
        {
        }

        protected override void TryLoadFromSettings()
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