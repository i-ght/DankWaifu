using DankWindowsWaifu.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DankWindowsWaifu.WPF
{
    public abstract class SettingCollection : SettingObj, INotifyPropertyChanged
    {
        /// <summary>
        /// Path to the file loaded
        /// </summary>
        public string FileName { get; set; }

        private string _valueLabel;

        public string ValueLabel
        {
            get { return _valueLabel; }
            set
            {
                _valueLabel = value;
                DankWaifu.Sys.Settings.Save(Key, FileName);
                OnPropertyChanged();
            }
        }

        protected abstract void TryLoadFromSettings();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}