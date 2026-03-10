using System.Runtime.InteropServices;

namespace LocalController.Services
{
    public class VolumeService
    {
        private const byte KeyEventKeyUp = 0x02;
        private const byte VkVolumeMute = 0xAD;
        private const byte VkVolumeDown = 0xAE;
        private const byte VkVolumeUp = 0xAF;

        private readonly object _sync = new object();
        private int _volume = 50;
        private bool _muted;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public int GetVolume()
        {
            lock (_sync)
            {
                return _volume;
            }
        }

        public bool IsMuted()
        {
            lock (_sync)
            {
                return _muted;
            }
        }

        public int SetVolume(int value)
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value > 100)
            {
                value = 100;
            }

            lock (_sync)
            {
                if (value > _volume)
                {
                    for (var i = _volume; i < value; i++)
                    {
                        SendKey(VkVolumeUp);
                    }
                }
                else if (value < _volume)
                {
                    for (var i = _volume; i > value; i--)
                    {
                        SendKey(VkVolumeDown);
                    }
                }

                _volume = value;
                if (_volume > 0)
                {
                    _muted = false;
                }

                return _volume;
            }
        }

        public int VolumeUp()
        {
            lock (_sync)
            {
                if (_volume < 100)
                {
                    SendKey(VkVolumeUp);
                    _volume += 2;
                    if (_volume > 100)
                    {
                        _volume = 100;
                    }
                    _muted = false;
                }

                return _volume;
            }
        }

        public int VolumeDown()
        {
            lock (_sync)
            {
                if (_volume > 0)
                {
                    SendKey(VkVolumeDown);
                    _volume -= 2;
                    if (_volume < 0)
                    {
                        _volume = 0;
                    }
                }

                return _volume;
            }
        }

        public bool ToggleMute()
        {
            lock (_sync)
            {
                SendKey(VkVolumeMute);
                _muted = !_muted;
                return _muted;
            }
        }

        private static void SendKey(byte keyCode)
        {
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KeyEventKeyUp, 0);
        }
    }
}
