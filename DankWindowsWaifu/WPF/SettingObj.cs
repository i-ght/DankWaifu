namespace DankWindowsWaifu.WPF
{
    public abstract class SettingObj
    {
        /// <summary>
        /// Key value of the setting to store in a file
        /// </summary>
        public abstract string Key { get; }

        /// <summary>
        /// Label to display on the UI
        /// </summary>
        public abstract string KeyLabel { get; }

        /// <summary>
        /// Type label to display on the UI
        /// </summary>
        public abstract string TypeLabel { get; }

        /// <summary>
        /// Value of the primitive or collection
        /// </summary>
        public abstract object ValueObj { get; protected set; }
    }
}