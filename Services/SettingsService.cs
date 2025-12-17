using ProjectLauncherTemplate.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ProjectLauncherTemplate.Services
{
    public class SettingsService
    {
        private const string SettingsFileName = "launcher_settings.json";
        private LauncherSettings _settings;

        public SettingsService()
        {
            _settings = LoadSettings();
        }

        public LauncherSettings Settings => _settings;

        private LauncherSettings LoadSettings()
        {
            if (File.Exists(SettingsFileName))
            {
                try
                {
                    var json = File.ReadAllText(SettingsFileName);
                    var loaded = JsonConvert.DeserializeObject<LauncherSettings>(json);
                    if (loaded != null) return loaded;
                }
                catch
                {
                    // Ignore errors, return default
                }
            }
            // Default path: Subfolder "Games" in current directory
            return new LauncherSettings
            {
                InstallPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games"),
                LocalVersion = "0.0.0"
            };
        }

        public void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(SettingsFileName, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void UpdateLocalVersion(string version)
        {
            _settings.LocalVersion = version;
            SaveSettings();
        }

        public void UpdateInstallPath(string path)
        {
            _settings.InstallPath = path;
            SaveSettings();
        }
    }
}
