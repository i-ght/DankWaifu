using DankWindowsWaifu.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DankWindowsWaifu.WPF
{
    public abstract class SettingPrimitive : SettingObj, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SettingPrimitive<T> : SettingPrimitive where T : IConvertible
    {
        public sealed override string Key { get; }
        public override string KeyLabel { get; }
        public override string TypeLabel { get; }
        public sealed override object ValueObj { get; protected set; }

        public T ValueLabel
        {
            get { return Value; }
            set
            {
                ValueObj = value;
                DankWaifu.Sys.Settings.Save(Key, value);
                OnPropertyChanged();
            }
        }

        public T Value => (T)ValueObj;

        public SettingPrimitive(string keyLabel, string keyValue, T defaultVal)
        {
            KeyLabel = keyLabel;
            Key = keyValue;
            TypeLabel = typeof(T).Name;

            if (DankWaifu.Sys.Settings.ContainsKey(Key))
                ValueObj = DankWaifu.Sys.Settings.Get<T>(Key);
            else
                ValueObj = defaultVal;
        }
    }
}