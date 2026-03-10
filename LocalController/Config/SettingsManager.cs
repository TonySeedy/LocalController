using System.IO;
using System.Text;
using System.Text.Json;

namespace LocalController.Config
{
    public class SettingsManager
    {
        private readonly string _settingsPath;
        private readonly JsonSerializerOptions _options;

        public SettingsManager(string baseDirectory)
        {
            _settingsPath = Path.Combine(baseDirectory, "settings.json");
            _options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
        }

        public AppSettings Load()
        {
            if (!File.Exists(_settingsPath))
            {
                var defaults = new AppSettings();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_settingsPath, Encoding.UTF8);
            try 
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(json, _options);
                if (settings == null)
                {
                    settings = new AppSettings();
                }

                if (settings.Port <= 0)
                {
                    settings.Port = 10000;
                }

                if (settings.RefreshInterval <= 0)
                {
                    settings.RefreshInterval = 10000;
                }

                return settings;
            }
            catch
            {
                return new AppSettings { Port = 10000, RefreshInterval = 10000 };
            }
        }

        public void Save(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, _options);
            File.WriteAllText(_settingsPath, json, Encoding.UTF8);
        }
    }
}
