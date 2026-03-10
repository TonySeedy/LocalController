using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace LocalController.Config
{
    public class SettingsManager
    {
        private readonly string _settingsPath;
        private readonly JavaScriptSerializer _serializer;

        public SettingsManager(string baseDirectory)
        {
            _settingsPath = Path.Combine(baseDirectory, "settings.json");
            _serializer = new JavaScriptSerializer();
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
            var settings = _serializer.Deserialize<AppSettings>(json);
            if (settings == null)
            {
                settings = new AppSettings();
            }

            if (settings.Port <= 0)
            {
                settings.Port = 5000;
            }

            if (settings.RefreshInterval <= 0)
            {
                settings.RefreshInterval = 10000;
            }

            return settings;
        }

        public void Save(AppSettings settings)
        {
            var json = _serializer.Serialize(settings);
            File.WriteAllText(_settingsPath, json, Encoding.UTF8);
        }
    }
}
