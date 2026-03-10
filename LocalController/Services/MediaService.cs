using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LocalController.Models;
using Windows.Media.Control;

namespace LocalController.Services
{
    public class MediaService
    {
        private const byte KeyEventKeyUp = 0x02;
        private const byte VkMediaPlayPause = 0xB3;
        private const byte VkMediaNextTrack = 0xB0;
        private const byte VkMediaPrevTrack = 0xB1;

        private readonly object _sync = new object();
        private bool _isPlaying;
        private GlobalSystemMediaTransportControlsSessionManager _smtcManager;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public MediaService()
        {
            InitializeSmtc();
        }

        private async void InitializeSmtc()
        {
            try
            {
                _smtcManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            }
            catch
            {
                // Ignore if GSMTC is not available
            }
        }

        public void Play()
        {
            SendKey(VkMediaPlayPause);
            lock (_sync)
            {
                _isPlaying = true;
            }
        }

        public void Pause()
        {
            SendKey(VkMediaPlayPause);
            lock (_sync)
            {
                _isPlaying = false;
            }
        }

        public void Next()
        {
            SendKey(VkMediaNextTrack);
        }

        public void Previous()
        {
            SendKey(VkMediaPrevTrack);
        }

        public void PlayPause()
        {
            SendKey(VkMediaPlayPause);
            lock (_sync)
            {
                _isPlaying = !_isPlaying;
            }
        }

        public MediaInfo GetMediaInfo()
        {
            lock (_sync)
            {
                // Try GSMTC first
                var mediaInfo = GetMediaInfoFromSmtc();
                if (mediaInfo != null)
                {
                    // Sync playing state if possible
                    _isPlaying = mediaInfo.playing;
                    return mediaInfo;
                }

                // Fallback to process title
                var title = GetMediaTitleFromProcess();
                return new MediaInfo
                {
                    title = title,
                    artist = "",
                    playing = _isPlaying
                };
            }
        }

        private MediaInfo GetMediaInfoFromSmtc()
        {
            if (_smtcManager == null) return null;

            try
            {
                var session = _smtcManager.GetCurrentSession();
                if (session != null)
                {
                    var mediaProperties = session.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
                    var playbackInfo = session.GetPlaybackInfo();

                    return new MediaInfo
                    {
                        title = mediaProperties.Title,
                        artist = mediaProperties.Artist,
                        playing = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing
                    };
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private static void SendKey(byte keyCode)
        {
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KeyEventKeyUp, 0);
        }

        private string GetMediaTitleFromProcess()
        {
            // List of known media players
            var players = new[] { "Spotify", "iTunes", "Music.UI", "chrome", "msedge" };

            foreach (var processName in players)
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var p in processes)
                {
                    if (!string.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        var title = p.MainWindowTitle;
                        
                        // Clean up titles
                        if (processName == "Spotify" && title != "Spotify" && title != "Spotify Premium")
                        {
                            return title; // "Artist - Song"
                        }
                        
                        if (processName == "chrome" || processName == "msedge")
                        {
                            if (title.Contains("- YouTube"))
                            {
                                return title.Replace("- YouTube - Google Chrome", "")
                                            .Replace("- YouTube - Microsoft Edge", "")
                                            .Trim();
                            }
                        }
                    }
                }
            }

            return "Unknown";
        }
    }
}
