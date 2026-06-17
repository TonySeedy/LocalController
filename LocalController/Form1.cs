using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using LocalController.Config;
using LocalController.Logger;
using LocalController.Server;
using LocalController.Services;

namespace LocalController
{
    public partial class Form1 : Form
    {
        private readonly SettingsManager _settingsManager;
        private readonly FileLogger _logger;
        private readonly ConfigService _configService;
        private readonly VolumeService _volumeService;
        private readonly MediaService _mediaService;
        private readonly SystemService _systemService;
        private readonly StatusService _statusService;

        private LocalHttpServer _server;
        private AppSettings _settings;
        private readonly bool _startHidden;

        public Form1(bool startHidden = false)
        {
            _startHidden = startHidden;
            InitializeComponent();

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _settingsManager = new SettingsManager(baseDirectory);
            _settings = _settingsManager.Load();
            _logger = new FileLogger(baseDirectory);
            _configService = new ConfigService(_settingsManager);
            _volumeService = new VolumeService();
            _mediaService = new MediaService();
            _systemService = new SystemService();
            _statusService = new StatusService(_volumeService, _mediaService, _systemService, _configService);

            // Cấu hình ban đầu cho UI từ settings
            _txtPort.Text = _settings.Port.ToString();
            _txtRefreshInterval.Text = _settings.RefreshInterval.ToString();
            _txtPassword.Text = _settings.Password;

            BindEvents();
            UpdateStatus(false);

            if (_startHidden)
            {
                // Khi hidden, Load event có thể không được gọi hoặc gọi chậm do Handle bị suppress.
                // Nên ta khởi động server trực tiếp luôn.
                StartServerInternal();
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (_startHidden && !this.IsHandleCreated)
            {
                CreateHandle();
                value = false;
            }
            base.SetVisibleCore(value);
        }

        private void BindEvents()
        {
            _btnStart.Click += OnStartClicked;
            _btnStop.Click += OnStopClicked;
            _logger.Logged += OnLogged;
            FormClosing += OnFormClosing;
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            if (!int.TryParse(_txtPort.Text, out var port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Port không hợp lệ");
                return;
            }

            if (!int.TryParse(_txtRefreshInterval.Text, out var refreshInterval) || refreshInterval < 1000)
            {
                MessageBox.Show("Refresh interval phải >= 1000");
                return;
            }

            _settings = new AppSettings
            {
                Port = port,
                RefreshInterval = refreshInterval,
                Password = _txtPassword.Text
            };
            _configService.SaveSettings(_settings);

            StartServerInternal();
        }

        private void StartServerInternal()
        {
            _server = new LocalHttpServer(
                _logger,
                _configService,
                _volumeService,
                _mediaService,
                _systemService,
                _statusService,
                AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                _server.Start(_settings);
                UpdateStatus(true);
            }
            catch (Exception ex)
            {
                if (!_startHidden)
                {
                    MessageBox.Show("Không thể start server: " + ex.Message);
                }
                _logger.Log("Failed to start server: " + ex.Message);
                UpdateStatus(false);
            }
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            if (_server != null)
            {
                _server.Stop();
            }
            UpdateStatus(false);
        }

        private void OnLogged(string line)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(OnLogged), line);
                return;
            }

            _txtLogs.AppendText(line + Environment.NewLine);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_server != null && _server.IsRunning)
            {
                _server.Stop();
            }
        }

        private void UpdateStatus(bool running)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(UpdateStatus), running);
                return;
            }

            _lblStatus.Text = running ? "RUNNING" : "STOPPED";
            _btnStart.Enabled = !running;
            _btnStop.Enabled = running;
            _txtPort.Enabled = !running;
            _txtRefreshInterval.Enabled = !running;
            _txtPassword.Enabled = !running;

            if (running)
            {
                _lblAddress.Text = "http://" + GetLocalIPv4() + ":" + _settings.Port;
            }
            else
            {
                _lblAddress.Text = "-";
            }
        }

        private static string GetLocalIPv4()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }
    }
}
