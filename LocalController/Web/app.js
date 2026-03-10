/**
 * AManoSoft Local Controller - Web Client
 * Modern ES6+ Implementation
 */

// --- Configuration ---
const CONFIG = {
    REFRESH_INTERVAL_MS: 10000,
    UPDATE_STEP_MS: 100,
    API_ENDPOINTS: {
        STATUS: '/api/status',
        MEDIA_PREV: '/api/media/previous',
        MEDIA_NEXT: '/api/media/next',
        MEDIA_PLAY_PAUSE: '/api/media/play-pause',
        VOLUME_SET: '/api/volume/set',
        VOLUME_UP: '/api/volume/up',
        VOLUME_DOWN: '/api/volume/down',
        VOLUME_MUTE: '/api/volume/mute',
        SYSTEM_LOCK: '/api/system/lock'
    }
};

// --- State Management ---
const store = {
    state: {
        refreshTimer: null,
        timeLeft: CONFIG.REFRESH_INTERVAL_MS,
        isUpdating: false,
        lastVolumeUpdate: 0,
        isConnected: true,
        isAuthRequired: false,
        password: localStorage.getItem('lc_password') || '',
        data: {
            media: { title: 'No Media Playing', artist: 'Unknown', playing: false },
            volume: 50,
            muted: false,
            locked: false
        }
    },
    
    // Simple subscribers for reactive UI
    listeners: new Set(),
    
    subscribe(listener) {
        this.listeners.add(listener);
    },
    
    notify() {
        this.listeners.forEach(listener => listener(this.state));
    },
    
    setState(newState) {
        this.state = { ...this.state, ...newState };
        this.notify();
    },
    
    updateData(data) {
        this.state.data = { ...this.state.data, ...data };
        this.notify();
    }
};

// --- UI Components / DOM Elements ---
const UI = {
    init() {
        this.cacheDOM();
        this.bindEvents();
        this.setupSubscriptions();
        this.startAutoRefresh();
        
        // Initial fetch
        Actions.refreshStatus();
    },

    showPasswordModal() {
        store.setState({ isAuthRequired: true });
        if (this.dom.passwordModal) {
            this.dom.passwordModal.classList.remove('hidden');
            if (this.dom.passwordDisplay) this.dom.passwordDisplay.value = '';
            if (this.dom.passwordError) this.dom.passwordError.classList.add('opacity-0');
        }
    },
    
    hidePasswordModal() {
        store.setState({ isAuthRequired: false });
        if (this.dom.passwordModal) {
            this.dom.passwordModal.classList.add('hidden');
        }
    },
    
    appendPassword(digit) {
        if (this.dom.passwordDisplay) {
            this.dom.passwordDisplay.value += digit;
            if (this.dom.passwordError) this.dom.passwordError.classList.add('opacity-0');
            
            if (this.dom.passwordDisplay.value.length >= 4) {
                this.submitPassword();
            }
        }
    },
    
    clearPassword() {
        if (this.dom.passwordDisplay) {
            const current = this.dom.passwordDisplay.value;
            this.dom.passwordDisplay.value = current.slice(0, -1);
            if (this.dom.passwordError) this.dom.passwordError.classList.add('opacity-0');
        }
    },

    async submitPassword() {
        if (!this.dom.passwordDisplay) return;
        const pwd = this.dom.passwordDisplay.value;
        
        store.setState({ password: pwd });
        localStorage.setItem('lc_password', pwd);
        
        const result = await Actions.api(CONFIG.API_ENDPOINTS.STATUS, "GET", null, true);
        
        if (result && result.success) {
            this.hidePasswordModal();
            Actions.refreshStatus();
        } else {
             if (this.dom.passwordError) {
                this.dom.passwordError.classList.remove('opacity-0');
                this.dom.passwordError.classList.remove('hidden');
            }
            this.dom.passwordDisplay.value = '';
        }
    },

    cacheDOM() {
        this.dom = {
            // Header
            connectionStatus: document.getElementById('connectionStatus'),
            refreshBtn: document.getElementById('manualRefreshBtn'),
            refreshIcon: document.getElementById('refreshIcon'),
            refreshProgress: document.getElementById('refreshProgress'),
            
            // Media
            mediaTitle: document.getElementById('mediaTitle'),
            mediaArtist: document.getElementById('mediaArtist'),
            btnPrev: document.getElementById('btnPrev'),
            btnNext: document.getElementById('btnNext'),
            btnPlayPause: document.getElementById('btnPlayPause'),
            iconPlayPause: document.getElementById('iconPlayPause'),
            
            // Volume
            volumeValue: document.getElementById('volumeValue'),
            volumeSlider: document.getElementById('volumeSlider'),
            volumeFill: document.getElementById('volumeFill'),
            volumeThumb: document.getElementById('volumeThumb'),
            btnMute: document.getElementById('btnMute'),
            iconMute: document.getElementById('iconMute'),
            btnVolDown: document.getElementById('btnVolumeDown'),
            btnVolUp: document.getElementById('btnVolumeUp'),
            
            // System
            btnLock: document.getElementById('btnLock'),
            lockState: document.getElementById('lockState'),
            lockStatusDot: document.getElementById('lockStatusDot'),

            // Password
            passwordModal: document.getElementById('password-modal'),
            passwordDisplay: document.getElementById('password-display'),
            passwordError: document.getElementById('password-error')
        };
        
        // SVG Circle circumference for progress
        this.circleCircumference = 2 * Math.PI * 16; // r=16
    },

    setupSubscriptions() {
        store.subscribe((state) => {
            this.renderConnection(state.isConnected);
            this.renderMedia(state.data.media);
            this.renderVolume(state.data.volume, state.data.muted, state.lastVolumeUpdate);
            this.renderSystem(state.data.locked);
            this.renderProgress(state.timeLeft);
            this.renderLoading(state.isUpdating);
        });
    },

    bindEvents() {
        const d = this.dom;
        
        // --- Native UI Enhancements ---
        // Release focus from buttons after click to mimic native app feel (prevents sticky hover/focus states)
        document.addEventListener('click', (e) => {
            const btn = e.target.closest('button');
            if (btn) {
                // Small timeout to ensure the click action registers before blurring
                setTimeout(() => btn.blur(), 50);
            }
        });

        // Prevent context menu on long press for a more app-like feel
        document.addEventListener('contextmenu', (e) => {
            if (!e.target.closest('input') && !e.target.closest('textarea')) {
                e.preventDefault();
            }
        });

        // Manual Refresh
        if (d.refreshBtn) d.refreshBtn.addEventListener('click', () => Actions.refreshStatus());

        // Media Controls
        if (d.btnPrev) d.btnPrev.addEventListener('click', () => Actions.mediaPrevious());
        if (d.btnNext) d.btnNext.addEventListener('click', () => Actions.mediaNext());
        if (d.btnPlayPause) d.btnPlayPause.addEventListener('click', () => Actions.mediaPlayPause());

        // Volume Controls
        if (d.volumeSlider) {
            d.volumeSlider.addEventListener('input', (e) => {
                const nextValue = Actions.normalizeVolume(e.target.value, store.state.data.volume);
                Actions.updateVolumeLocal(nextValue);
            });
            d.volumeSlider.addEventListener('change', async (e) => {
                const nextValue = Actions.normalizeVolume(e.target.value, store.state.data.volume);
                await Actions.commitVolume(nextValue);
            });
        }
        
        if (d.btnVolDown) d.btnVolDown.addEventListener('click', () => Actions.volumeStep(-2));
        if (d.btnVolUp) d.btnVolUp.addEventListener('click', () => Actions.volumeStep(2));
        if (d.btnMute) d.btnMute.addEventListener('click', () => Actions.volumeMute());

        // System Controls
        if (d.btnLock) d.btnLock.addEventListener('click', () => Actions.systemLock());
    },

    startAutoRefresh() {
        if (this.timer) clearInterval(this.timer);
        
        this.timer = setInterval(() => {
            const { isUpdating, timeLeft, isAuthRequired } = store.state;
            
            if (!isUpdating && !isAuthRequired) {
                let newTimeLeft = timeLeft - CONFIG.UPDATE_STEP_MS;
                
                if (newTimeLeft <= 0) {
                    Actions.refreshStatus();
                    newTimeLeft = 0;
                }
                
                store.setState({ timeLeft: newTimeLeft });
            }
        }, CONFIG.UPDATE_STEP_MS);
    },

    // --- Renderers ---
    
    renderConnection(isConnected) {
        const el = this.dom.connectionStatus;
        if (!el) return;
        
        if (isConnected) {
            el.className = "inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-700 mt-1 transition-colors";
            el.innerHTML = '<span class="w-1.5 h-1.5 rounded-full bg-green-600 animate-pulse"></span> Connected';
        } else {
            el.className = "inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700 mt-1 transition-colors";
            el.innerHTML = '<span class="w-1.5 h-1.5 rounded-full bg-red-600"></span> Disconnected';
        }
    },

    renderMedia(media) {
        if (this.dom.mediaTitle) this.dom.mediaTitle.textContent = media.title || 'No Media Playing';
        if (this.dom.mediaArtist) this.dom.mediaArtist.textContent = media.artist || 'Unknown Artist';
        if (this.dom.iconPlayPause) this.dom.iconPlayPause.textContent = media.playing ? 'pause' : 'play_arrow';
    },

    renderVolume(volume, muted, lastUpdate) {
        // Skip update if user interacted recently (prevent jumping)
        if (Date.now() - lastUpdate < 1000 && lastUpdate !== 0) return;

        if (this.dom.volumeValue) this.dom.volumeValue.textContent = `${volume}%`;
        if (this.dom.volumeSlider) this.dom.volumeSlider.value = volume;
        if (this.dom.iconMute) this.dom.iconMute.textContent = muted ? 'volume_off' : 'volume_up';
        
        this.updateVolumeVisuals(volume);
        
        if (this.dom.iconMute) {
            if (muted) {
                 this.dom.iconMute.classList.add('text-red-500');
            } else {
                 this.dom.iconMute.classList.remove('text-red-500');
            }
        }
    },

    updateVolumeVisuals(percent) {
        // Correct offset for native range input behavior
        // Formula: calc(percent% + (10px - percent * 0.2px))
        // This aligns the visual thumb center with the native input thumb center
        const offset = `calc(${percent}% + (${10 - percent * 0.2}px))`;
        
        if (this.dom.volumeFill) this.dom.volumeFill.style.width = offset;
        if (this.dom.volumeThumb) this.dom.volumeThumb.style.left = offset;
    },

    renderSystem(locked) {
        if (this.dom.lockState) this.dom.lockState.textContent = locked ? 'Locked' : 'Unlocked';
        if (this.dom.lockStatusDot) {
            if (locked) {
                this.dom.lockStatusDot.className = "w-2 h-2 rounded-full bg-rose-500";
            } else {
                this.dom.lockStatusDot.className = "w-2 h-2 rounded-full bg-green-500";
            }
        }
        
        if (this.dom.btnLock) {
            this.dom.btnLock.disabled = locked;
            if (locked) {
                this.dom.btnLock.classList.add('opacity-50', 'cursor-not-allowed', 'grayscale');
                this.dom.btnLock.classList.remove('hover:bg-rose-100', 'active:scale-95');
            } else {
                this.dom.btnLock.classList.remove('opacity-50', 'cursor-not-allowed', 'grayscale');
                this.dom.btnLock.classList.add('hover:bg-rose-100', 'active:scale-95');
            }
        }
    },

    renderProgress(timeLeft) {
        if (!this.dom.refreshProgress) return;
        const percent = (timeLeft / CONFIG.REFRESH_INTERVAL_MS);
        const offset = this.circleCircumference * (1 - percent);
        this.dom.refreshProgress.style.strokeDashoffset = offset;
        this.dom.refreshProgress.style.strokeDasharray = this.circleCircumference;
    },

    renderLoading(isUpdating) {
        if (!this.dom.refreshIcon) return;
        if (isUpdating) {
            this.dom.refreshIcon.classList.add('animate-spin');
        } else {
            this.dom.refreshIcon.classList.remove('animate-spin');
        }
    }
};

// --- Actions / API ---
const Actions = {
    normalizeVolume(value, fallback = 50) {
        const parsed = Number(value);
        if (!Number.isFinite(parsed)) return Math.min(100, Math.max(0, Number(fallback) || 50));
        return Math.min(100, Math.max(0, parsed));
    },

    async api(path, method = "GET", body = null, suppressAuthModal = false) {
        const options = { 
            method, 
            headers: {
                "X-Auth-Password": store.state.password
            } 
        };
        if (body !== null) {
            options.headers["Content-Type"] = "application/json";
            options.body = JSON.stringify(body);
        }

        try {
            const response = await fetch(path, options);
            if (response.status === 401) {
                if (!suppressAuthModal) UI.showPasswordModal();
                throw new Error('Unauthorized');
            }
            if (!response.ok) throw new Error('Network response was not ok');
            const data = await response.json();
            
            store.setState({ isConnected: true });
            return data;
        } catch (error) {
            console.error('API Error:', error);
            if (error.message !== 'Unauthorized') {
                store.setState({ isConnected: false });
            }
            return { success: false };
        }
    },

    async refreshStatus() {
        if (store.state.isUpdating) return;
        
        store.setState({ isUpdating: true });
        
        const result = await this.api(CONFIG.API_ENDPOINTS.STATUS);
        
        // Simulate minimum delay for visual feedback
        setTimeout(() => {
            if (result.success && result.data) {
                store.updateData(result.data);
                // Reset timer
                store.setState({ 
                    timeLeft: CONFIG.REFRESH_INTERVAL_MS,
                    isUpdating: false 
                });
            } else {
                 store.setState({ isUpdating: false });
            }
        }, 500);
    },

    async mediaPrevious() {
        await this.api(CONFIG.API_ENDPOINTS.MEDIA_PREV, "POST");
        setTimeout(() => this.refreshStatus(), 200);
    },

    async mediaNext() {
        await this.api(CONFIG.API_ENDPOINTS.MEDIA_NEXT, "POST");
        setTimeout(() => this.refreshStatus(), 200);
    },

    async mediaPlayPause() {
        // Optimistic update
        const current = store.state.data.media;
        store.updateData({ media: { ...current, playing: !current.playing } });
        
        await this.api(CONFIG.API_ENDPOINTS.MEDIA_PLAY_PAUSE, "POST");
        setTimeout(() => this.refreshStatus(), 200);
    },

    updateVolumeLocal(value) {
        const safeValue = this.normalizeVolume(value, store.state.data.volume);
        store.setState({ lastVolumeUpdate: Date.now() });
        store.updateData({ volume: safeValue });
        
        // Update visual slider immediately using UI helper
        if (typeof UI !== 'undefined' && UI.updateVolumeVisuals) {
            UI.updateVolumeVisuals(safeValue);
        } else {
            // Fallback if UI not ready (shouldn't happen)
            const offset = `calc(${safeValue}% + (${10 - safeValue * 0.2}px))`;
            const fill = document.getElementById('volumeFill');
            const thumb = document.getElementById('volumeThumb');
            if (fill) fill.style.width = offset;
            if (thumb) thumb.style.left = offset;
        }
    },

    async commitVolume(value) {
        const safeValue = this.normalizeVolume(value, store.state.data.volume);
        this.updateVolumeLocal(safeValue);
        await this.api(CONFIG.API_ENDPOINTS.VOLUME_SET, "POST", { value: safeValue });
    },

    async volumeStep(delta) {
        const currentVol = store.state.data.volume;
        const newVol = Math.min(100, Math.max(0, currentVol + delta));
        
        this.updateVolumeLocal(newVol);
        
        const endpoint = delta > 0 ? CONFIG.API_ENDPOINTS.VOLUME_UP : CONFIG.API_ENDPOINTS.VOLUME_DOWN;
        await this.api(endpoint, "POST");
    },

    async volumeMute() {
        // Optimistic
        store.updateData({ muted: !store.state.data.muted });
        await this.api(CONFIG.API_ENDPOINTS.VOLUME_MUTE, "POST");
        setTimeout(() => this.refreshStatus(), 200);
    },

    async systemLock() {
        await this.api(CONFIG.API_ENDPOINTS.SYSTEM_LOCK, "POST");
        setTimeout(() => this.refreshStatus(), 500);
    }
};

// --- Bootstrap ---
document.addEventListener('DOMContentLoaded', () => {
    UI.init();
});

// Global Password Handlers
window.appendPassword = (d) => UI.appendPassword(d);
window.clearPassword = () => UI.clearPassword();
window.submitPassword = () => UI.submitPassword();
