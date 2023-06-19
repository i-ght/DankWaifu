using System.IO;
using FileStream = DankWaifu.Collections.FileStream;

namespace DankWindowsWaifu.WPF
{
    public sealed class SettingFileStream : SettingCollection
    {
        public override string Key { get; }
        public override string KeyLabel { get; }
        public override string TypeLabel { get; }
        public override object ValueObj { get; protected set; }

        public SettingFileStream(string key)
        {
            KeyLabel = key;
            Key = key;
            TypeLabel = "List";
            ValueObj = new FileStream();
            TryLoadFromSettings();
        }

        public SettingFileStream(string keyLabel, string keyValue)
        {
            KeyLabel = keyLabel;
            Key = keyValue;
            TypeLabel = "List";
            ValueObj = new FileStream();
            TryLoadFromSettings();
        }

        public FileStream Value => (FileStream)ValueObj;

        protected override void TryLoadFromSettings()
        {
            if (!DankWaifu.Sys.Settings.ContainsKey(Key))
                return;

            var fileName = DankWaifu.Sys.Settings.Get<string>(Key);
            if (!File.Exists(fileName))
                return;

            Load(fileName);
        }

        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            FileName = fileName;
            Value.GotLineCount += Value_OnGotLineCount;
            Value.Open(fileName);
            ValueLabel = $"[counting...] | {fileName}";
        }

        private void Value_OnGotLineCount(object sender, long e)
        {
            ValueLabel = $"[{Value.Count:N0}] | {FileName}";
            Value.GotLineCount -= Value_OnGotLineCount;
        }
    }
}