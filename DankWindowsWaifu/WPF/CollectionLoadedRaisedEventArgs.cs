using System;
using System.Collections.Generic;

namespace DankWindowsWaifu.WPF
{
    public class CollectionLoadedRaisedEventArgs : EventArgs
    {
        public CollectionLoadedRaisedEventArgs(
            string settingKey,
            string fileName,
            int count,
            List<string> collection)
        {
            SettingKey = settingKey;
            FileName = fileName;
            Count = count;
            Collection = collection;
        }

        public string SettingKey { get; }
        public string FileName { get; }
        public int Count { get; }
        public List<string> Collection { get; }
    }
}