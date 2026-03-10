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

        private Label _lblStatus;
        private Label _lblAddress;
        private TextBox _txtPort;
        private TextBox _txtRefreshInterval;
        private Button _btnStart;
        private Button _btnStop;
        private TextBox _txtLogs;

        public Form1()
        {
            InitializeComponent();
            Text = "PC Remote Server";
            Width = 760;
            Height = 560;
            StartPosition = FormStartPosition.CenterScreen;

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _settingsManager = new SettingsManager(baseDirectory);
            _settings = _settingsManager.Load();
            _logger = new FileLogger(baseDirectory);
            _configService = new ConfigService(_settingsManager);
            _volumeService = new VolumeService();
            _mediaService = new MediaService();
            _systemService = new SystemService();
            _statusService = new StatusService(_volumeService, _mediaService, _systemService, _configService);

            BuildLayout();
            BindEvents();
            UpdateStatus(false);
        }

        private void BuildLayout()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(12)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            panel.Controls.Add(new Label { Text = "Status:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 0);
            _lblStatus = new Label { Text = "STOPPED", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            panel.Controls.Add(_lblStatus, 1, 0);

            panel.Controls.Add(new Label { Text = "Địa chỉ truy cập:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 1);
            _lblAddress = new Label { Text = "-", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            panel.Controls.Add(_lblAddress, 1, 1);

            panel.Controls.Add(new Label { Text = "Port:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 2);
            _txtPort = new TextBox { Dock = DockStyle.Fill, Text = _settings.Port.ToString() };
            panel.Controls.Add(_txtPort, 1, 2);

            panel.Controls.Add(new Label { Text = "Refresh interval (ms):", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 3);
            _txtRefreshInterval = new TextBox { Dock = DockStyle.Fill, Text = _settings.RefreshInterval.ToString() };
            panel.Controls.Add(_txtRefreshInterval, 1, 3);

            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _btnStart = new Button { Text = "Start", Width = 100, Height = 30 };
            _btnStop = new Button { Text = "Stop", Width = 100, Height = 30, Enabled = false };
            buttonPanel.Controls.Add(_btnStart);
            buttonPanel.Controls.Add(_btnStop);
            panel.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 0, 4);
            panel.Controls.Add(buttonPanel, 1, 4);

            panel.Controls.Add(new Label { Text = "Logs:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 6);
            _txtLogs = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            panel.Controls.Add(_txtLogs, 0, 7);
            panel.SetColumnSpan(_txtLogs, 2);

            Controls.Add(panel);
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
                RefreshInterval = refreshInterval
            };
            _configService.SaveSettings(_settings);

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
                MessageBox.Show("Không thể start server: " + ex.Message);
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
            _lblStatus.Text = running ? "RUNNING" : "STOPPED";
            _btnStart.Enabled = !running;
            _btnStop.Enabled = running;
            _txtPort.Enabled = !running;
            _txtRefreshInterval.Enabled = !running;

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
