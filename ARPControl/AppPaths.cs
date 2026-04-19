using System;
using System.IO;
using System.Text.Json;

namespace ARPControl
{
    public static class AppPaths
    {
        public static string BaseFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ARPControl");

        public static string SettingsFile => Path.Combine(BaseFolder, "settings.json");

        public static AppSettings LoadSettings()
        {
            try
            {
                Directory.CreateDirectory(BaseFolder);

                if (!File.Exists(SettingsFile))
                    return new AppSettings();

                string json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void SaveSettings(AppSettings settings)
        {
            Directory.CreateDirectory(BaseFolder);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
    }
}