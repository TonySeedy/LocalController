# 1. Mục tiêu hệ thống

Ứng dụng C# chạy trên PC với các chức năng:

### Server

* Chạy **HTTP server local**
* Serve **API**
* Serve **Web UI để điều khiển từ điện thoại**

### Điều khiển hệ thống

* Media control
* Volume control
* Lock PC

### Trạng thái

* Volume %
* Mute
* Lock state
* Media đang phát (title / artist / playing)

### Desktop App

* Start / Stop service
* Chỉnh port
* Chỉnh refresh interval
* Hiển thị log
* Ghi log file

---

# 2. Kiến trúc tổng thể

```
Phone Browser
      │
      │ HTTP
      ▼
PC Remote Server
      │
      ├── API Layer
      │
      ├── Services
      │     ├─ MediaService
      │     ├─ VolumeService
      │     ├─ SystemService
      │     ├─ StatusService
      │     └─ ConfigService
      │
      ├── Logger
      │
      ├── Web UI
      │
      └── Desktop UI (WinForms)
```

---

# 3. Công nghệ sử dụng

| Component      | Tech                               |
| -------------- | ---------------------------------- |
| Desktop UI     | WinForms                           |
| HTTP server    | ASP.NET Core Minimal API           |
| Volume control | CoreAudio API                      |
| Media control  | SendInput                          |
| Media metadata | GlobalSystemMediaTransportControls |
| Lock PC        | LockWorkStation                    |
| Settings       | JSON                               |
| Logger         | Custom file logger                 |

---

# 4. Cấu trúc project

```
PcRemoteServer
│
├─ Controllers
│   ├─ MediaController.cs
│   ├─ VolumeController.cs
│   ├─ SystemController.cs
│   ├─ StatusController.cs
│   └─ ConfigController.cs
│
├─ Services
│   ├─ MediaService.cs
│   ├─ VolumeService.cs
│   ├─ SystemService.cs
│   ├─ StatusService.cs
│   └─ ConfigService.cs
│
├─ Logger
│   └─ FileLogger.cs
│
├─ Config
│   └─ SettingsManager.cs
│
├─ Web
│   ├─ index.html
│   ├─ app.js
│   └─ style.css
│
├─ logs
│
└─ Program.cs
```

---

# 5. Settings system

File:

```
settings.json
```

Ví dụ:

```json
{
  "port": 5000,
  "refreshInterval": 10000
}
```

Startup flow:

```
Start App
   │
Load settings.json
   │
Init logger
   │
Start HTTP server
```

---

# 6. Logging system

### Log format

```
YYYY-MM-DD HH:mm:ss
```

Ví dụ

```
2026-03-11 22:10:01 Server started on port 5000
2026-03-11 22:10:05 GET /api/status
2026-03-11 22:10:08 POST /api/media/next
```

---

### Log file

```
logs/
   2026-03-11.log
   2026-03-12.log
```

Rotation theo ngày.

---

# 7. Chuẩn response API

Tất cả API trả về format chung.

### Thành công

```json
{
  "success": true,
  "message": "Volume updated",
  "data": {}
}
```

### Lỗi

```json
{
  "success": false,
  "message": "Invalid request"
}
```

---

# 8. API Status (quan trọng nhất)

```
GET /api/status
```

Response:

```json
{
  "success": true,
  "data": {
    "volume": 42,
    "muted": false,
    "locked": false,
    "media": {
      "title": "Numb",
      "artist": "Linkin Park",
      "playing": true
    },
    "refreshInterval": 10000
  }
}
```

Mobile chỉ cần gọi API này để update UI.

---

# 9. Media API

### Play

```
POST /api/media/play
```

Response

```json
{
  "success": true,
  "message": "Media play command sent"
}
```

---

### Pause

```
POST /api/media/pause
```

---

### Next

```
POST /api/media/next
```

---

### Previous

```
POST /api/media/previous
```

---

### Toggle (fallback cho media key)

```
POST /api/media/play-pause
```

---

# 10. Volume API

### Get volume

```
GET /api/volume
```

---

### Set volume %

```
POST /api/volume/set
```

Body

```json
{
  "value": 70
}
```

Response

```json
{
  "success": true,
  "data": {
    "volume": 70
  }
}
```

---

### Volume up

```
POST /api/volume/up
```

---

### Volume down

```
POST /api/volume/down
```

---

### Mute

```
POST /api/volume/mute
```

---

# 11. System API

### Lock PC

```
POST /api/system/lock
```

---

### Lock state

```
GET /api/system/lock-state
```

Response

```json
{
  "success": true,
  "data": {
    "locked": true
  }
}
```

---

# 12. Config API

### Lấy config

```
GET /api/config
```

Response

```json
{
  "success": true,
  "data": {
    "refreshInterval": 10000
  }
}
```

---

# 13. Utility API

### Ping

```
GET /api/ping
```

Response

```json
{
  "success": true,
  "message": "server alive"
}
```

---

# 14. Web UI cho mobile

Server serve:

```
GET /
```

Mobile interface:

```
MEDIA
⏮  ⏯  ⏭

Title: Linkin Park - Numb

VOLUME
[-]  42%  [+]

[ slider ]

SYSTEM
Lock PC

Status: Unlocked

Refresh
[ Refresh ]
Auto refresh: 10s
```

---

# 15. Auto refresh logic

Mobile JS:

```javascript
let refreshInterval = 10000;

async function refreshStatus() {
  const res = await fetch("/api/status");
  const data = await res.json();

  updateUI(data.data);
}

setInterval(refreshStatus, refreshInterval);
```

Manual refresh:

```javascript
refreshStatus();
```

---

# 16. Status service

`StatusService` gom trạng thái từ nhiều service.

```
StatusService
     │
     ├── VolumeService
     ├── MediaService
     └── SystemService
```

Pseudo flow:

```
GetVolume()
GetMuteState()
GetLockState()
GetMediaInfo()

Return combined object
```

---

# 17. Desktop UI

App đơn giản.

```
--------------------------------
PC Remote Server

Status: RUNNING

Port:
[ 5000 ]

Refresh interval (ms)
[ 10000 ]

[ Start ]
[ Stop ]

Logs
--------------------------------
2026-03-11 Server started
...
--------------------------------
```

---

# 18. Networking

Server bind:

```
0.0.0.0
```

Ví dụ truy cập từ điện thoại:

```
http://192.168.1.10:5000
```

---

# 19. Flow request

```
Phone press Volume Up
        │
POST /api/volume/up
        │
VolumeController
        │
VolumeService
        │
CoreAudio API
        │
Return response
```

---

# 20. Danh sách API cuối cùng

### Status

```
GET /api/status
```

---

### Media

```
POST /api/media/play
POST /api/media/pause
POST /api/media/play-pause
POST /api/media/next
POST /api/media/previous
```

---

### Volume

```
GET  /api/volume
POST /api/volume/set
POST /api/volume/up
POST /api/volume/down
POST /api/volume/mute
```

---

### System

```
POST /api/system/lock
GET  /api/system/lock-state
```

---

### Config

```
GET /api/config
```

---

### Utility

```
GET /api/ping
```

---
