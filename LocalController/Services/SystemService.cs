using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LocalController.Services
{
    public class SystemService
    {
        private readonly object _sync = new object();
        private bool _isLocked;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool LockWorkStation();

        public SystemService()
        {
            SystemEvents.SessionSwitch += OnSessionSwitch;
        }

        public bool LockPc()
        {
            lock (_sync)
            {
                var success = LockWorkStation();
                if (success)
                {
                    _isLocked = true;
                }
                return success;
            }
        }

        public bool IsLocked()
        {
            lock (_sync)
            {
                return _isLocked;
            }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            lock (_sync)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    _isLocked = true;
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    _isLocked = false;
                }
            }
        }
    }
}
