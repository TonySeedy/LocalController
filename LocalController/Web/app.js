let refreshTimer = null;

async function api(path, method = "GET", body = null) {
    const options = { method, headers: {} };
    if (body !== null) {
        options.headers["Content-Type"] = "application/json";
        options.body = JSON.stringify(body);
    }

    const response = await fetch(path, options);
    return response.json();
}

function renderStatus(status) {
    document.getElementById("volumeValue").textContent = `${status.volume}%`;
    document.getElementById("volumeSlider").value = status.volume;
    document.getElementById("mediaTitle").textContent = `Title: ${status.media.artist} - ${status.media.title}`;
    document.getElementById("lockState").textContent = `Status: ${status.locked ? "Locked" : "Unlocked"}`;
    document.getElementById("refreshValue").textContent = `Auto refresh: ${Math.floor(status.refreshInterval / 1000)}s`;
    startRefresh(status.refreshInterval);
}

async function refreshStatus() {
    const result = await api("/api/status");
    if (result.success && result.data) {
        renderStatus(result.data);
    }
}

function startRefresh(intervalMs) {
    if (refreshTimer !== null) {
        clearInterval(refreshTimer);
    }

    refreshTimer = setInterval(refreshStatus, intervalMs);
}

async function bindActions() {
    document.getElementById("btnPrev").addEventListener("click", async () => {
        await api("/api/media/previous", "POST");
        await refreshStatus();
    });

    document.getElementById("btnPlayPause").addEventListener("click", async () => {
        await api("/api/media/play-pause", "POST");
        await refreshStatus();
    });

    document.getElementById("btnNext").addEventListener("click", async () => {
        await api("/api/media/next", "POST");
        await refreshStatus();
    });

    document.getElementById("btnVolumeDown").addEventListener("click", async () => {
        await api("/api/volume/down", "POST");
        await refreshStatus();
    });

    document.getElementById("btnVolumeUp").addEventListener("click", async () => {
        await api("/api/volume/up", "POST");
        await refreshStatus();
    });

    document.getElementById("btnMute").addEventListener("click", async () => {
        await api("/api/volume/mute", "POST");
        await refreshStatus();
    });

    document.getElementById("btnLock").addEventListener("click", async () => {
        await api("/api/system/lock", "POST");
    });

    document.getElementById("btnRefresh").addEventListener("click", refreshStatus);

    document.getElementById("volumeSlider").addEventListener("change", async (event) => {
        const value = Number(event.target.value);
        await api("/api/volume/set", "POST", { value });
        await refreshStatus();
    });
}

bindActions().then(refreshStatus);
