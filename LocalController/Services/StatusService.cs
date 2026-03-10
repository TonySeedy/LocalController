using LocalController.Models;

namespace LocalController.Services
{
    public class StatusService
    {
        private readonly VolumeService _volumeService;
        private readonly MediaService _mediaService;
        private readonly SystemService _systemService;
        private readonly ConfigService _configService;

        public StatusService(
            VolumeService volumeService,
            MediaService mediaService,
            SystemService systemService,
            ConfigService configService)
        {
            _volumeService = volumeService;
            _mediaService = mediaService;
            _systemService = systemService;
            _configService = configService;
        }

        public StatusSnapshot GetStatus()
        {
            var settings = _configService.GetSettings();
            return new StatusSnapshot
            {
                volume = _volumeService.GetVolume(),
                muted = _volumeService.IsMuted(),
                locked = _systemService.IsLocked(),
                media = _mediaService.GetMediaInfo(),
                refreshInterval = settings.RefreshInterval
            };
        }
    }
}
