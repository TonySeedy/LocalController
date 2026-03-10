using LocalController.Config;

namespace LocalController.Services
{
    public class ConfigService
    {
        private readonly SettingsManager _settingsManager;
        private readonly object _sync = new object();
        private AppSettings _settings;

        public ConfigService(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _settings = _settingsManager.Load();
        }

        public AppSettings GetSettings()
        {
            lock (_sync)
            {
                return new AppSettings
                {
                    Port = _settings.Port,
                    RefreshInterval = _settings.RefreshInterval
                };
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            lock (_sync)
            {
                _settings = settings;
                _settingsManager.Save(settings);
            }
        }
    }
}
