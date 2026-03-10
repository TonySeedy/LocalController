using System.Runtime.InteropServices;
using LocalController.Models;

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

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

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
                return new MediaInfo
                {
                    title = "Unknown",
                    artist = "Unknown",
                    playing = _isPlaying
                };
            }
        }

        private static void SendKey(byte keyCode)
        {
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KeyEventKeyUp, 0);
        }
    }
}
