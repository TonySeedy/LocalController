using System;
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

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public int GetVolume()
        {
            lock (_sync)
            {
                var vol = GetMasterVolume();
                return (int)Math.Round(vol * 100);
            }
        }

        public bool IsMuted()
        {
            lock (_sync)
            {
                return GetMute();
            }
        }

        public int SetVolume(int value)
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;

            lock (_sync)
            {
                SetMasterVolume(value / 100f);
                // Return actual volume from system to be sure
                var vol = GetMasterVolume();
                return (int)Math.Round(vol * 100);
            }
        }

        public int VolumeUp()
        {
            lock (_sync)
            {
                // Simulate key press to show OSD
                SendKey(VkVolumeUp);
                
                // Return updated volume (approximate or wait)
                System.Threading.Thread.Sleep(100);
                var vol = GetMasterVolume();
                return (int)Math.Round(vol * 100);
            }
        }

        public int VolumeDown()
        {
            lock (_sync)
            {
                // Simulate key press to show OSD
                SendKey(VkVolumeDown);

                // Return updated volume
                System.Threading.Thread.Sleep(100);
                var vol = GetMasterVolume();
                return (int)Math.Round(vol * 100);
            }
        }

        public bool ToggleMute()
        {
            lock (_sync)
            {
                // Simulate key press to show OSD
                SendKey(VkVolumeMute);

                // Return updated state
                System.Threading.Thread.Sleep(100);
                return GetMute();
            }
        }

        private static void SendKey(byte keyCode)
        {
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KeyEventKeyUp, 0);
        }

        // --- Core Audio API Helpers ---

        private static float GetMasterVolume()
        {
            IAudioEndpointVolume volume = null;
            try
            {
                volume = GetDefaultAudioEndpointVolume();
                float level;
                volume.GetMasterVolumeLevelScalar(out level);
                return level;
            }
            finally
            {
                if (volume != null) Marshal.ReleaseComObject(volume);
            }
        }

        private static void SetMasterVolume(float level)
        {
            IAudioEndpointVolume volume = null;
            try
            {
                volume = GetDefaultAudioEndpointVolume();
                volume.SetMasterVolumeLevelScalar(level, Guid.Empty);
            }
            finally
            {
                if (volume != null) Marshal.ReleaseComObject(volume);
            }
        }

        private static bool GetMute()
        {
            IAudioEndpointVolume volume = null;
            try
            {
                volume = GetDefaultAudioEndpointVolume();
                bool mute;
                volume.GetMute(out mute);
                return mute;
            }
            finally
            {
                if (volume != null) Marshal.ReleaseComObject(volume);
            }
        }

        private static void SetMute(bool mute)
        {
            IAudioEndpointVolume volume = null;
            try
            {
                volume = GetDefaultAudioEndpointVolume();
                volume.SetMute(mute, Guid.Empty);
            }
            finally
            {
                if (volume != null) Marshal.ReleaseComObject(volume);
            }
        }

        private static IAudioEndpointVolume GetDefaultAudioEndpointVolume()
        {
            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;
            try
            {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);
                
                object o;
                // CLSCTX_ALL = 23 (1 | 2 | 4 | 16)
                device.Activate(typeof(IAudioEndpointVolume).GUID, 23, IntPtr.Zero, out o);
                return (IAudioEndpointVolume)o;
            }
            finally
            {
                if (device != null) Marshal.ReleaseComObject(device);
                if (enumerator != null) Marshal.ReleaseComObject(enumerator);
            }
        }

        // --- COM Interfaces & Classes ---

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class MMDeviceEnumerator
        {
        }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            void EnumAudioEndpoints(EDataFlow dataFlow, uint dwStateMask, out object ppDevices);
            void GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
            void GetDevice(string pwstrId, out IMMDevice ppDevice);
            void RegisterEndpointNotificationCallback(object pClient);
            void UnregisterEndpointNotificationCallback(object pClient);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            void Activate([MarshalAs(UnmanagedType.LPStruct)] Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            void OpenPropertyStore(int stgmAccess, out object ppProperties);
            void GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            void GetState(out int pdwState);
        }

        [ComImport]
        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioEndpointVolume
        {
            void RegisterControlChangeNotify(object pNotify);
            void UnregisterControlChangeNotify(object pNotify);
            void GetChannelCount(out int pnChannelCount);
            void SetMasterVolumeLevel(float fLevelDB, Guid pguidEventContext);
            void SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);
            void GetMasterVolumeLevel(out float pfLevelDB);
            void GetMasterVolumeLevelScalar(out float pfLevel);
            void SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid pguidEventContext);
            void SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid pguidEventContext);
            void GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            void GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
            void SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, Guid pguidEventContext);
            void GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
            void GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
            void VolumeStepUp(Guid pguidEventContext);
            void VolumeStepDown(Guid pguidEventContext);
            void QueryHardwareSupport(out uint pdwHardwareSupportMask);
            void GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        internal enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
            EDataFlow_enum_count
        }

        internal enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
            ERole_enum_count
        }
    }
}
