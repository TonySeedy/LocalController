# LocalController

A small Windows desktop app that runs a local HTTP server so you can control your PC from your phone's browser over the LAN.

## Features

- **Media control**: play, pause, play/pause, next, previous. Shows the current track (title, artist, playing state).
- **Volume control**: up, down, mute, set to an exact percentage.
- **System**: lock the PC and show the current lock state.
- **Web UI**: a mobile-friendly page served by the app. No install needed on the phone.
- **Optional PIN**: protect the API with a password.
- **Desktop app**: start/stop the server, change the port and refresh interval, view logs. Logs are also written to a file.
- **Single instance**: only one copy of the app runs at a time.
- **Hidden startup**: start minimized with `--hidden` or `--silent`.

## Requirements

- Windows 10 (19041) or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build & run

```bash
dotnet build LocalController.sln
```

Then run `LocalController/bin/Debug/net8.0-windows10.0.19041.0/LocalController.exe`, or open `LocalController.sln` in Visual Studio.

Open `http://<your-pc-ip>:5000` on your phone (same Wi-Fi network).

### LAN access

If the server can only bind to `localhost` (you'll see a warning in the log), run the app as Administrator once, or reserve the URL from an Admin terminal:

```bash
netsh http add urlacl url=http://+:5000/ user=%USERNAME%
```

You may also need to allow the port through Windows Firewall.

## Auto-start on login

Run `CreateTaskSchedule.bat` as Administrator. It creates a Task Scheduler entry that starts the app hidden 1 minute after you log on. Edit `APP_PATH` in the script if you use a Release build.

## Configuration

Settings are saved to `settings.json` next to the executable:

| Key | Default | Description |
|-----|---------|-------------|
| `Port` | `5000` | HTTP server port |
| `RefreshInterval` | `10000` | Web UI status refresh interval (ms) |
| `Password` | `""` | API PIN. Empty = no auth |

## API

When a password is set, every `/api/*` request except `/api/ping` must send the `X-Auth-Password` header.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/ping` | Health check |
| POST | `/api/auth` | Check password. Body: `{"password": "1234"}` |
| GET | `/api/status` | Volume, mute, lock state, current media |
| GET | `/api/config` | Client config (refresh interval, port) |
| POST | `/api/media/play` | Play |
| POST | `/api/media/pause` | Pause |
| POST | `/api/media/play-pause` | Toggle play/pause |
| POST | `/api/media/next` | Next track |
| POST | `/api/media/previous` | Previous track |
| GET | `/api/volume` | Current volume |
| POST | `/api/volume/up` | Volume up |
| POST | `/api/volume/down` | Volume down |
| POST | `/api/volume/mute` | Toggle mute |
| POST | `/api/volume/set` | Set volume. Body: `{"value": 50}` |
| POST | `/api/system/lock` | Lock the PC |
| GET | `/api/system/lock-state` | Is the PC locked |

## Security note

This app is meant for a trusted home network. Traffic is plain HTTP and the PIN is stored in plain text, so don't expose the port to the internet.
