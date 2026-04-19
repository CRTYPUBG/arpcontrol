using Microsoft.Win32;
using System.Windows.Forms;

namespace ARPControl
{
    public static class StartupManager
    {
        private const string AppName = "ARPControl";

        public static bool IsEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
            return key?.GetValue(AppName) != null;
        }

        public static void SetEnabled(bool enabled)
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;

            if (enabled)
            {
                string exe = Application.ExecutablePath;
                key.SetValue(AppName, $"\"{exe}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
    }
}