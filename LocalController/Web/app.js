// Constants
const CIRCLE_RADIUS = 16;
const CIRCLE_CIRCUMFERENCE = 2 * Math.PI * CIRCLE_RADIUS;
const REFRESH_INTERVAL_MS = 10000; // 10 seconds default
const UPDATE_STEP_MS = 100; // Update UI every 100ms

// DOM Elements
const elements = {
    mediaTitle: document.getElementById('mediaTitle'),
    mediaArtist: document.getElementById('mediaArtist'),
    btnPrev: document.getElementById('btnPrev'),
    btnPlayPause: document.getElementById('btnPlayPause'),
    iconPlayPause: document.getElementById('iconPlayPause'),
    btnNext: document.getElementById('btnNext'),
    volumeValue: document.getElementById('volumeValue'),
    volumeSlider: document.getElementById('volumeSlider'),
    btnVolumeDown: document.getElementById('btnVolumeDown'),
    btnVolumeUp: document.getElementById('btnVolumeUp'),
    btnMute: document.getElementById('btnMute'),
    iconMute: document.getElementById('iconMute'),
    btnLock: document.getElementById('btnLock'),
    lockState: document.getElementById('lockState'),
    connectionStatus: document.getElementById('connectionStatus'),
    progressCircle: document.querySelector('.progress-ring__circle'),
    refreshContainer: document.querySelector('.refresh-container'),
    manualRefreshBtn: document.getElementById('manualRefreshBtn')
};

// State
let state = {
    refreshTimer: null,
    timeLeft: REFRESH_INTERVAL_MS,
    isUpdating: false,
    lastVolumeUpdate: 0
};

// Initialization
function init() {
    setupEventListeners();
    setupProgressCircle();
    startAutoRefresh();
    refreshStatus();
}

function setupProgressCircle() {
    elements.progressCircle.style.strokeDasharray = `${CIRCLE_CIRCUMFERENCE} ${CIRCLE_CIRCUMFERENCE}`;
    elements.progressCircle.style.strokeDashoffset = CIRCLE_CIRCUMFERENCE;
}

function setProgress(percent) {
    const offset = CIRCLE_CIRCUMFERENCE - (percent / 100) * CIRCLE_CIRCUMFERENCE;
    elements.progressCircle.style.strokeDashoffset = offset;
}

// API Helper
async function api(path, method = "GET", body = null) {
    const options = { method, headers: {} };
    if (body !== null) {
        options.headers["Content-Type"] = "application/json";
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(path, options);
        if (!response.ok) throw new Error('Network response was not ok');
        const data = await response.json();
        updateConnectionStatus(true);
        return data;
    } catch (error) {
        console.error('API Error:', error);
        updateConnectionStatus(false);
        return { success: false };
    }
}

function updateConnectionStatus(isConnected) {
    if (isConnected) {
        elements.connectionStatus.textContent = 'Connected';
        elements.connectionStatus.classList.remove('disconnected');
    } else {
        elements.connectionStatus.textContent = 'Disconnected';
        elements.connectionStatus.classList.add('disconnected');
    }
}

// Data Handling
async function refreshStatus() {
    if (state.isUpdating) return;
    state.isUpdating = true;

    // Trigger visual refresh
    elements.refreshContainer.classList.add('spinning');

    const result = await api("/api/status");
    
    // Simulate minimal delay for visual feedback if response is too fast
    setTimeout(() => {
        elements.refreshContainer.classList.remove('spinning');
        
        if (result.success && result.data) {
            updateUI(result.data);
            triggerSuccessAnimation();
        }
        
        // Reset timer
        state.timeLeft = REFRESH_INTERVAL_MS;
        state.isUpdating = false;
    }, 500);
}

function triggerSuccessAnimation() {
    elements.refreshContainer.classList.add('success');
    setProgress(100); // Full circle green
    
    setTimeout(() => {
        elements.refreshContainer.classList.remove('success');
        // Resume countdown visual from full
    }, 1000);
}

function updateUI(data) {
    // Media
    const media = data.media || { title: 'Unknown', artist: 'Unknown', playing: false };
    elements.mediaTitle.textContent = media.title || 'No Media Playing';
    elements.mediaArtist.textContent = media.artist || 'Unknown Artist';
    elements.iconPlayPause.textContent = media.playing ? 'pause' : 'play_arrow';

    // Volume - Only update if no recent local interaction (1s grace period)
    if (Date.now() - state.lastVolumeUpdate > 1000) {
        elements.volumeValue.textContent = `${data.volume}%`;
        elements.volumeSlider.value = data.volume;
        elements.iconMute.textContent = data.muted ? 'volume_off' : 'volume_up';
    }
    
    // System
    elements.lockState.textContent = data.locked ? 'Locked' : 'Unlocked';
}

// Timer Logic
function startAutoRefresh() {
    if (state.refreshTimer) clearInterval(state.refreshTimer);

    state.refreshTimer = setInterval(() => {
        if (!state.isUpdating) {
            state.timeLeft -= UPDATE_STEP_MS;
            
            // Calculate percentage for progress ring
            // We want it to go from 0 to 100 (full) as time decreases? 
            // Or full to empty? User asked for "vòng tròn xoay nhẹ nhẹ cho đến hết" -> Full to empty usually
            // Then "sau đó xanh và full xoay lại tiếp" -> Green full circle on success
            
            const percent = (state.timeLeft / REFRESH_INTERVAL_MS) * 100;
            setProgress(percent);

            if (state.timeLeft <= 0) {
                refreshStatus();
            }
        }
    }, UPDATE_STEP_MS);
}

// Event Listeners
function setupEventListeners() {
    // Media
    elements.btnPrev.addEventListener('click', () => apiAndRefresh("/api/media/previous", "POST"));
    elements.btnNext.addEventListener('click', () => apiAndRefresh("/api/media/next", "POST"));
    elements.btnPlayPause.addEventListener('click', () => {
        // Optimistic UI update
        const isPlaying = elements.iconPlayPause.textContent === 'pause';
        elements.iconPlayPause.textContent = isPlaying ? 'play_arrow' : 'pause';
        apiAndRefresh("/api/media/play-pause", "POST");
    });

    // Volume
    elements.volumeSlider.addEventListener('input', (e) => {
        state.lastVolumeUpdate = Date.now();
        elements.volumeValue.textContent = `${e.target.value}%`;
    });
    
    elements.volumeSlider.addEventListener('change', async (e) => {
        state.lastVolumeUpdate = Date.now();
        await api("/api/volume/set", "POST", { value: Number(e.target.value) });
        // Don't full refresh immediately to avoid jumping slider, just silent update
    });

    elements.btnVolumeDown.addEventListener('click', () => {
        updateVolumeOptimistically(-2);
        apiAndRefresh("/api/volume/down", "POST");
    });

    elements.btnVolumeUp.addEventListener('click', () => {
        updateVolumeOptimistically(2);
        apiAndRefresh("/api/volume/up", "POST");
    });

    function updateVolumeOptimistically(delta) {
        state.lastVolumeUpdate = Date.now();
        let currentVol = Number(elements.volumeSlider.value);
        let newVol = Math.min(100, Math.max(0, currentVol + delta));
        elements.volumeSlider.value = newVol;
        elements.volumeValue.textContent = `${newVol}%`;
    }

    elements.btnMute.addEventListener('click', () => {
        // Optimistic UI update
        const isMuted = elements.iconMute.textContent === 'volume_off';
        elements.iconMute.textContent = isMuted ? 'volume_up' : 'volume_off';
        apiAndRefresh("/api/volume/mute", "POST");
    });

    // System
    elements.btnLock.addEventListener('click', () => api("/api/system/lock", "POST"));

    // Manual Refresh
    elements.manualRefreshBtn.addEventListener('click', () => {
        refreshStatus();
    });
}

async function apiAndRefresh(path, method, body) {
    await api(path, method, body);
    // Short delay to let system process command
    setTimeout(refreshStatus, 200);
}

// Start
document.addEventListener('DOMContentLoaded', init);