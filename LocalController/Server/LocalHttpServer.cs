using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LocalController.Config;
using LocalController.Logger;
using LocalController.Models;
using LocalController.Services;

namespace LocalController.Server
{
    public class LocalHttpServer
    {
        private readonly FileLogger _logger;
        private readonly ConfigService _configService;
        private readonly VolumeService _volumeService;
        private readonly MediaService _mediaService;
        private readonly SystemService _systemService;
        private readonly StatusService _statusService;
        private readonly string _baseDirectory;
        private readonly JavaScriptSerializer _serializer;

        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private Task _serverTask;

        public bool IsRunning { get; private set; }

        public LocalHttpServer(
            FileLogger logger,
            ConfigService configService,
            VolumeService volumeService,
            MediaService mediaService,
            SystemService systemService,
            StatusService statusService,
            string baseDirectory)
        {
            _logger = logger;
            _configService = configService;
            _volumeService = volumeService;
            _mediaService = mediaService;
            _systemService = systemService;
            _statusService = statusService;
            _baseDirectory = baseDirectory;
            _serializer = new JavaScriptSerializer();
        }

        public void Start(AppSettings settings)
        {
            if (IsRunning)
            {
                return;
            }

            var prefixes = new[]
            {
                "http://127.0.0.1:" + settings.Port + "/",
                "http://localhost:" + settings.Port + "/"
            };

            Exception lastException = null;
            foreach (var prefix in prefixes)
            {
                try
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add(prefix);
                    _listener.Start();
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (_listener != null)
                    {
                        try
                        {
                            _listener.Close();
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (_listener == null || !_listener.IsListening)
            {
                var message = "Không thể start HTTP listener ở port " + settings.Port +
                              ". Nếu lỗi Access is denied, mở CMD/Powershell Admin và chạy: " +
                              "netsh http add urlacl url=http://localhost:" + settings.Port + "/ user=%USERNAME%";
                throw new InvalidOperationException(message, lastException);
            }

            _cts = new CancellationTokenSource();
            _serverTask = Task.Run(() => ListenLoop(_cts.Token));
            IsRunning = true;
            _logger.Log("Server started on port " + settings.Port);
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            try
            {
                _cts.Cancel();
                _listener.Stop();
                _listener.Close();
                _serverTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
            }
            finally
            {
                IsRunning = false;
                _logger.Log("Server stopped");
            }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch
                {
                    break;
                }

                _ = Task.Run(() => HandleRequest(context), token);
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var path = request.Url.AbsolutePath;
            var method = request.HttpMethod.ToUpperInvariant();
            _logger.Log(method + " " + path);

            try
            {
                if (path == "/")
                {
                    ServeStatic(response, "index.html", "text/html; charset=utf-8");
                    return;
                }

                if (path == "/app.js")
                {
                    ServeStatic(response, "app.js", "application/javascript; charset=utf-8");
                    return;
                }

                if (path == "/style.css")
                {
                    ServeStatic(response, "style.css", "text/css; charset=utf-8");
                    return;
                }

                if (path == "/api/ping" && method == "GET")
                {
                    WriteJson(response, Success("server alive", null));
                    return;
                }

                if (path == "/api/status" && method == "GET")
                {
                    WriteJson(response, Success(null, _statusService.GetStatus()));
                    return;
                }

                if (path == "/api/media/play" && method == "POST")
                {
                    _mediaService.Play();
                    WriteJson(response, Success("Media play command sent", null));
                    return;
                }

                if (path == "/api/media/pause" && method == "POST")
                {
                    _mediaService.Pause();
                    WriteJson(response, Success("Media pause command sent", null));
                    return;
                }

                if (path == "/api/media/play-pause" && method == "POST")
                {
                    _mediaService.PlayPause();
                    WriteJson(response, Success("Media toggle command sent", null));
                    return;
                }

                if (path == "/api/media/next" && method == "POST")
                {
                    _mediaService.Next();
                    WriteJson(response, Success("Media next command sent", null));
                    return;
                }

                if (path == "/api/media/previous" && method == "POST")
                {
                    _mediaService.Previous();
                    WriteJson(response, Success("Media previous command sent", null));
                    return;
                }

                if (path == "/api/volume" && method == "GET")
                {
                    WriteJson(response, Success(null, new
                    {
                        volume = _volumeService.GetVolume(),
                        muted = _volumeService.IsMuted()
                    }));
                    return;
                }

                if (path == "/api/volume/up" && method == "POST")
                {
                    var value = _volumeService.VolumeUp();
                    WriteJson(response, Success("Volume increased", new { volume = value }));
                    return;
                }

                if (path == "/api/volume/down" && method == "POST")
                {
                    var value = _volumeService.VolumeDown();
                    WriteJson(response, Success("Volume decreased", new { volume = value }));
                    return;
                }

                if (path == "/api/volume/mute" && method == "POST")
                {
                    var muted = _volumeService.ToggleMute();
                    WriteJson(response, Success("Mute toggled", new { muted }));
                    return;
                }

                if (path == "/api/volume/set" && method == "POST")
                {
                    var body = ReadBody(request);
                    var payload = _serializer.Deserialize<Dictionary<string, object>>(body);
                    if (payload == null || !payload.ContainsKey("value"))
                    {
                        WriteJson(response, Error("Invalid request"));
                        return;
                    }

                    var value = Convert.ToInt32(payload["value"]);
                    var result = _volumeService.SetVolume(value);
                    WriteJson(response, Success("Volume updated", new { volume = result }));
                    return;
                }

                if (path == "/api/system/lock" && method == "POST")
                {
                    var success = _systemService.LockPc();
                    if (success)
                    {
                        WriteJson(response, Success("PC locked", null));
                    }
                    else
                    {
                        WriteJson(response, Error("Lock command failed"));
                    }
                    return;
                }

                if (path == "/api/system/lock-state" && method == "GET")
                {
                    WriteJson(response, Success(null, new { locked = _systemService.IsLocked() }));
                    return;
                }

                if (path == "/api/config" && method == "GET")
                {
                    var settings = _configService.GetSettings();
                    WriteJson(response, Success(null, new
                    {
                        refreshInterval = settings.RefreshInterval,
                        port = settings.Port
                    }));
                    return;
                }

                WriteJson(response, Error("Not found"), 404);
            }
            catch (Exception ex)
            {
                _logger.Log("Error: " + ex.Message);
                WriteJson(response, Error("Internal server error"), 500);
            }
        }

        private void ServeStatic(HttpListenerResponse response, string fileName, string contentType)
        {
            var path = Path.Combine(_baseDirectory, "Web", fileName);
            if (!File.Exists(path))
            {
                WriteJson(response, Error("Not found"), 404);
                return;
            }

            var bytes = File.ReadAllBytes(path);
            response.StatusCode = 200;
            response.ContentType = contentType;
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        private static string ReadBody(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        private void WriteJson(HttpListenerResponse response, ApiResponse apiResponse, int statusCode = 200)
        {
            var json = _serializer.Serialize(apiResponse);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        private static ApiResponse Success(string message, object data)
        {
            return new ApiResponse
            {
                success = true,
                message = message,
                data = data
            };
        }

        private static ApiResponse Error(string message)
        {
            return new ApiResponse
            {
                success = false,
                message = message,
                data = null
            };
        }
    }
}
