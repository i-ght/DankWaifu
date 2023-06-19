using Microsoft.Win32;
using System.Windows;

namespace DankWindowsWaifu.WPF
{
    public static class WPFHelpers
    {
        public static string SelectFile(Window owner, string title)
        {
            var ofd = new OpenFileDialog
            {
                Title = $"Load {title}"
            };
            var result = ofd.ShowDialog(owner);
            if (result == false)
                return string.Empty;

            return ofd.FileName;
        }
    }
}