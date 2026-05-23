/**
 * WinLockTimer Web UI — 前端核心逻辑
 * 纯 JavaScript SPA，Token 认证 + 状态轮询 + 控制操作
 */

(function () {
    'use strict';

    // =========================================
    // 常量与配置
    // =========================================
    const TOKEN_KEY = 'winlocktimer_access_token';
    const POLL_INTERVAL = 1000; // 状态轮询间隔（毫秒）
    const RING_CIRCUMFERENCE = 2 * Math.PI * 90; // SVG 圆环周长 (r=90)

    // =========================================
    // DOM 元素引用
    // =========================================
    const $ = (sel) => document.querySelector(sel);
    const $$ = (sel) => document.querySelectorAll(sel);

    const dom = {
        // 页面
        authPage: $('#auth-page'),
        mainPage: $('#main-page'),
        dashboardView: $('#dashboard-view'),
        historyView: $('#history-view'),

        // 认证
        tokenInput: $('#token-input'),
        authSubmit: $('#auth-submit'),
        authError: $('#auth-error'),
        togglePassword: $('#toggle-password'),
        eyeIcon: $('#eye-icon'),
        eyeOffIcon: $('#eye-off-icon'),

        // 导航
        navDashboard: $('#nav-dashboard'),
        navHistory: $('#nav-history'),
        logoutBtn: $('#logout-btn'),

        // 倒计时
        timerRemaining: $('#timer-remaining'),
        timerStatusText: $('#timer-status-text'),
        timerStatusBadge: $('#timer-status-badge'),
        ringProgress: $('#ring-progress'),

        // 控制
        inputHours: $('#input-hours'),
        inputMinutes: $('#input-minutes'),
        selectAccount: $('#select-account'),
        btnStart: $('#btn-start'),
        btnPause: $('#btn-pause'),
        btnReset: $('#btn-reset'),

        // 信息
        infoState: $('#info-state'),
        infoTotal: $('#info-total'),
        infoAccount: $('#info-account'),
        infoWindowsSession: $('#info-windows-session'),
        infoProgress: $('#info-progress'),

        // 历史
        filterStart: $('#filter-start'),
        filterEnd: $('#filter-end'),
        filterAccount: $('#filter-account'),
        btnQuery: $('#btn-query'),
        historyTbody: $('#history-tbody'),

        // Toast
        toastContainer: $('#toast-container'),
    };

    // =========================================
    // 状态管理
    // =========================================
    let pollTimer = null;
    let accounts = [];
    let currentStatus = null;

    // =========================================
    // 工具函数
    // =========================================

    /** 发起 API 请求 */
    async function apiRequest(path, options = {}) {
        const token = localStorage.getItem(TOKEN_KEY);
        const headers = {
            'Content-Type': 'application/json',
            ...(options.headers || {}),
        };

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(path, {
            ...options,
            headers,
        });

        return response;
    }

    /** 显示 Toast 消息 */
    function showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        dom.toastContainer.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('toast-exit');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    /** 格式化秒为 HH:MM:SS */
    function formatSeconds(totalSeconds) {
        const h = Math.floor(totalSeconds / 3600);
        const m = Math.floor((totalSeconds % 3600) / 60);
        const s = totalSeconds % 60;
        return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
    }

    /** 格式化日期为 YYYY-MM-DD */
    function formatDate(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    // =========================================
    // 认证逻辑
    // =========================================

    /** 验证 Token */
    async function verifyToken(token) {
        try {
            const resp = await fetch('/api/auth/verify', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ token }),
            });
            const data = await resp.json();
            return data.success === true;
        } catch {
            return false;
        }
    }

    /** 初始化认证流程 */
    async function initAuth() {
        const savedToken = localStorage.getItem(TOKEN_KEY);
        if (savedToken) {
            const valid = await verifyToken(savedToken);
            if (valid) {
                showMainPage();
                return;
            }
            // Token 无效，清除
            localStorage.removeItem(TOKEN_KEY);
        }
        showAuthPage();
    }

    function showAuthPage() {
        dom.authPage.style.display = '';
        dom.mainPage.style.display = 'none';
        stopPolling();
        dom.tokenInput.focus();
    }

    function showMainPage() {
        dom.authPage.style.display = 'none';
        dom.mainPage.style.display = '';
        dom.mainPage.classList.add('fade-in');
        loadAccounts();
        startPolling();
    }

    // 认证表单提交
    dom.authSubmit.addEventListener('click', async () => {
        const token = dom.tokenInput.value.trim();
        if (!token) {
            showAuthError('请输入 Access Token');
            return;
        }

        // 显示加载状态
        setAuthLoading(true);

        const valid = await verifyToken(token);
        if (valid) {
            localStorage.setItem(TOKEN_KEY, token);
            dom.authError.style.display = 'none';
            showMainPage();
        } else {
            showAuthError('Token 无效，请检查后重试');
        }

        setAuthLoading(false);
    });

    // 回车提交
    dom.tokenInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') dom.authSubmit.click();
    });

    // 密码显示/隐藏切换
    dom.togglePassword.addEventListener('click', () => {
        const isPassword = dom.tokenInput.type === 'password';
        dom.tokenInput.type = isPassword ? 'text' : 'password';
        dom.eyeIcon.style.display = isPassword ? 'none' : '';
        dom.eyeOffIcon.style.display = isPassword ? '' : 'none';
    });

    function showAuthError(msg) {
        dom.authError.textContent = msg;
        dom.authError.style.display = '';
    }

    function setAuthLoading(loading) {
        const btnText = dom.authSubmit.querySelector('.btn-text');
        const btnLoader = dom.authSubmit.querySelector('.btn-loader');
        dom.authSubmit.disabled = loading;
        btnText.style.display = loading ? 'none' : '';
        btnLoader.style.display = loading ? '' : 'none';
    }

    // =========================================
    // 退出登录
    // =========================================
    dom.logoutBtn.addEventListener('click', () => {
        localStorage.removeItem(TOKEN_KEY);
        showAuthPage();
        showToast('已退出登录', 'info');
    });

    // =========================================
    // 页面导航
    // =========================================
    dom.navDashboard.addEventListener('click', () => switchView('dashboard'));
    dom.navHistory.addEventListener('click', () => switchView('history'));

    function switchView(view) {
        // 更新导航高亮
        $$('.nav-btn[data-page]').forEach((btn) => {
            btn.classList.toggle('active', btn.dataset.page === view);
        });

        if (view === 'dashboard') {
            dom.dashboardView.style.display = '';
            dom.historyView.style.display = 'none';
        } else if (view === 'history') {
            dom.dashboardView.style.display = 'none';
            dom.historyView.style.display = '';
            loadHistory();
        }
    }

    // =========================================
    // 状态轮询
    // =========================================

    function startPolling() {
        if (pollTimer) return;
        pollStatus(); // 立即执行一次
        pollTimer = setInterval(pollStatus, POLL_INTERVAL);
    }

    function stopPolling() {
        if (pollTimer) {
            clearInterval(pollTimer);
            pollTimer = null;
        }
    }

    async function pollStatus() {
        try {
            const resp = await apiRequest('/api/timer/status');
            if (resp.status === 401) {
                // Token 失效
                localStorage.removeItem(TOKEN_KEY);
                showAuthPage();
                showToast('登录已过期，请重新认证', 'error');
                return;
            }
            const data = await resp.json();
            updateTimerUI(data);
            currentStatus = data;
        } catch (err) {
            // 网络错误，静默处理
            console.warn('轮询失败:', err);
        }
    }

    /** 更新倒计时 UI */
    function updateTimerUI(status) {
        // 倒计时数字
        dom.timerRemaining.textContent = status.remainingDisplay || formatSeconds(status.remainingSeconds);

        // 状态文本
        dom.timerStatusText.textContent = status.status;

        // 状态徽标
        updateStatusBadge(status);

        // 环形进度
        updateRing(status);

        // 数字颜色
        updateDigitColor(status);

        // 按钮状态
        updateButtons(status);

        // 信息面板
        updateInfoPanel(status);
    }

    function updateStatusBadge(status) {
        const badge = dom.timerStatusBadge;
        badge.className = 'status-badge';

        if (status.isRunning && !status.isPaused) {
            badge.textContent = '运行中';
            badge.classList.add('status-running');
        } else if (status.isRunning && status.isPaused) {
            badge.textContent = '已暂停';
            badge.classList.add('status-paused');
        } else {
            badge.textContent = '待机';
            badge.classList.add('status-idle');
        }
    }

    function updateRing(status) {
        const ring = dom.ringProgress;
        if (!ring) return;

        let progress = 0;
        if (status.totalSeconds > 0) {
            progress = status.remainingSeconds / status.totalSeconds;
        }

        const offset = RING_CIRCUMFERENCE * (1 - progress);
        ring.setAttribute('stroke-dashoffset', offset);

        // 根据剩余时间改变颜色
        let color;
        const remainMin = status.remainingSeconds / 60;
        if (remainMin <= 5) {
            color = '#EF4444'; // 红色
        } else if (remainMin <= 10) {
            color = '#F59E0B'; // 橙色
        } else {
            color = '#6C63FF'; // 主色
        }
        ring.style.stroke = color;
    }

    function updateDigitColor(status) {
        const digits = dom.timerRemaining;
        const remainMin = status.remainingSeconds / 60;
        if (status.isRunning) {
            if (remainMin <= 5) {
                digits.style.textShadow = '0 0 20px rgba(239, 68, 68, 0.5)';
                digits.style.color = '#FCA5A5';
            } else if (remainMin <= 10) {
                digits.style.textShadow = '0 0 20px rgba(245, 158, 11, 0.5)';
                digits.style.color = '#FDE68A';
            } else {
                digits.style.textShadow = '0 0 20px rgba(108, 99, 255, 0.3)';
                digits.style.color = '#F0F0FF';
            }
        } else {
            digits.style.textShadow = '';
            digits.style.color = '';
        }
    }

    function updateButtons(status) {
        if (status.isRunning) {
            dom.btnStart.disabled = true;
            dom.btnPause.disabled = false;
            dom.btnReset.disabled = false;
            dom.inputHours.disabled = true;
            dom.inputMinutes.disabled = true;
            dom.selectAccount.disabled = true;

            // 暂停/继续按钮文本
            const pauseSpan = dom.btnPause.querySelector('span');
            if (status.isPaused) {
                pauseSpan.textContent = '继续';
                dom.btnPause.className = 'btn btn-success';
            } else {
                pauseSpan.textContent = '暂停';
                dom.btnPause.className = 'btn btn-warning';
            }
        } else {
            dom.btnStart.disabled = false;
            dom.btnPause.disabled = true;
            dom.btnReset.disabled = true;
            dom.inputHours.disabled = false;
            dom.inputMinutes.disabled = false;
            dom.selectAccount.disabled = false;

            // 重置暂停按钮
            const pauseSpan = dom.btnPause.querySelector('span');
            pauseSpan.textContent = '暂停';
            dom.btnPause.className = 'btn btn-warning';
        }
    }

    function updateInfoPanel(status) {
        // 运行状态
        if (status.isRunning && !status.isPaused) {
            dom.infoState.textContent = '运行中';
            dom.infoState.style.color = '#34D399';
        } else if (status.isRunning && status.isPaused) {
            dom.infoState.textContent = '已暂停';
            dom.infoState.style.color = '#FBBF24';
        } else {
            dom.infoState.textContent = '待机';
            dom.infoState.style.color = '';
        }

        // 总时长
        dom.infoTotal.textContent = status.totalSeconds > 0 ? formatSeconds(status.totalSeconds) : '--:--:--';

        // 当前账户
        const acc = accounts.find((a) => a.id === status.currentAccountId);
        dom.infoAccount.textContent = acc ? acc.username : '无';

        // Windows 锁屏状态
        dom.infoWindowsSession.textContent = status.windowsSessionState || (status.isWindowsLocked ? '已锁屏' : '已解锁');
        dom.infoWindowsSession.style.color = status.isWindowsLocked ? '#FCA5A5' : '';

        // 进度条
        let pct = 0;
        if (status.totalSeconds > 0) {
            pct = ((status.totalSeconds - status.remainingSeconds) / status.totalSeconds) * 100;
        }
        dom.infoProgress.style.width = `${Math.min(pct, 100)}%`;
    }

    // =========================================
    // 控制操作
    // =========================================

    // 密码模态框 DOM 引用
    const modalDom = {
        overlay: $('#password-modal'),
        input: $('#modal-password-input'),
        confirm: $('#modal-confirm'),
        cancel: $('#modal-cancel'),
        error: $('#modal-error'),
        actionText: $('#modal-action-text'),
    };

    /**
     * 弹出密码输入模态框
     * 返回 Promise<string|null>，用户输入密码返回字符串，取消返回 null
     */
    function promptPassword(actionDesc) {
        return new Promise((resolve) => {
            modalDom.actionText.textContent = `请输入家长密码以${actionDesc}`;
            modalDom.input.value = '';
            modalDom.error.style.display = 'none';
            modalDom.overlay.style.display = '';

            // 聚焦输入框
            setTimeout(() => modalDom.input.focus(), 100);

            // 清理旧事件（避免重复绑定）
            const newConfirm = modalDom.confirm.cloneNode(true);
            modalDom.confirm.parentNode.replaceChild(newConfirm, modalDom.confirm);
            modalDom.confirm = newConfirm;

            const newCancel = modalDom.cancel.cloneNode(true);
            modalDom.cancel.parentNode.replaceChild(newCancel, modalDom.cancel);
            modalDom.cancel = newCancel;

            function closeModal(result) {
                modalDom.overlay.style.display = 'none';
                modalDom.input.removeEventListener('keydown', onKeydown);
                resolve(result);
            }

            function onKeydown(e) {
                if (e.key === 'Enter') {
                    const pwd = modalDom.input.value.trim();
                    if (pwd) closeModal(pwd);
                } else if (e.key === 'Escape') {
                    closeModal(null);
                }
            }

            modalDom.input.addEventListener('keydown', onKeydown);

            newConfirm.addEventListener('click', () => {
                const pwd = modalDom.input.value.trim();
                if (!pwd) {
                    modalDom.error.textContent = '请输入密码';
                    modalDom.error.style.display = '';
                    return;
                }
                closeModal(pwd);
            });

            newCancel.addEventListener('click', () => closeModal(null));
        });
    }

    /** 显示模态框错误信息 */
    function showModalError(msg) {
        modalDom.error.textContent = msg;
        modalDom.error.style.display = '';
    }

    /**
     * 发送需要密码验证的 API 请求
     * 先尝试无密码请求，如果返回 403 则弹窗让用户输入密码后重试
     */
    async function sendProtectedRequest(endpoint, actionDesc, successMsg, extraBody = {}) {
        try {
            // 第一次尝试：不带密码
            const resp = await apiRequest(endpoint, {
                method: 'POST',
                body: JSON.stringify(extraBody),
            });
            const data = await resp.json();

            if (data.success) {
                showToast(successMsg, 'success');
                return true;
            }

            // 需要密码验证
            if (resp.status === 403) {
                const password = await promptPassword(actionDesc);
                if (!password) return false; // 用户取消

                // 带密码重试
                const retryResp = await apiRequest(endpoint, {
                    method: 'POST',
                    body: JSON.stringify({ ...extraBody, password }),
                });
                const retryData = await retryResp.json();

                if (retryData.success) {
                    showToast(successMsg, 'success');
                    return true;
                } else {
                    showToast(retryData.message || '操作失败', 'error');
                    return false;
                }
            }

            showToast(data.message || '操作失败', 'error');
            return false;
        } catch (err) {
            showToast('请求失败，请检查连接', 'error');
            return false;
        }
    }

    dom.btnStart.addEventListener('click', async () => {
        const hours = parseInt(dom.inputHours.value, 10) || 0;
        const minutes = parseInt(dom.inputMinutes.value, 10) || 0;
        const accountId = parseInt(dom.selectAccount.value, 10) || -1;

        if (hours === 0 && minutes === 0) {
            showToast('请设置有效的时间', 'error');
            return;
        }

        if (accounts.length > 0 && accountId <= 0) {
            showToast('请选择计时用户', 'error');
            dom.selectAccount.focus();
            return;
        }

        try {
            const resp = await apiRequest('/api/timer/start', {
                method: 'POST',
                body: JSON.stringify({ hours, minutes, accountId }),
            });
            const data = await resp.json();
            if (data.success) {
                showToast('倒计时已启动', 'success');
            } else {
                showToast(data.message || '启动失败', 'error');
            }
        } catch (err) {
            showToast('请求失败，请检查连接', 'error');
        }
    });

    dom.btnPause.addEventListener('click', async () => {
        const isPaused = currentStatus && currentStatus.isPaused;
        const endpoint = isPaused ? '/api/timer/resume' : '/api/timer/pause';
        const actionDesc = isPaused ? '继续倒计时' : '暂停倒计时';
        const successMsg = isPaused ? '倒计时已继续' : '倒计时已暂停';

        await sendProtectedRequest(endpoint, actionDesc, successMsg);
    });

    dom.btnReset.addEventListener('click', async () => {
        await sendProtectedRequest('/api/timer/reset', '重置倒计时', '倒计时已重置');
    });

    // =========================================
    // 账户加载
    // =========================================

    async function loadAccounts() {
        try {
            const resp = await apiRequest('/api/accounts');
            if (resp.ok) {
                const data = await resp.json();
                accounts = data.accounts || [];
                populateAccountSelects();
            }
        } catch (err) {
            console.warn('加载账户失败:', err);
        }
    }

    function populateAccountSelects() {
        // 控制台账户选择
        dom.selectAccount.innerHTML = '';
        if (accounts.length > 0) {
            const placeholder = document.createElement('option');
            placeholder.value = '-1';
            placeholder.textContent = '请选择用户';
            placeholder.disabled = true;
            placeholder.selected = true;
            dom.selectAccount.appendChild(placeholder);
        } else {
            const empty = document.createElement('option');
            empty.value = '-1';
            empty.textContent = '无可用用户';
            dom.selectAccount.appendChild(empty);
        }

        accounts.forEach((acc) => {
            const opt = document.createElement('option');
            opt.value = acc.id;
            opt.textContent = acc.username;
            dom.selectAccount.appendChild(opt);
        });

        // 历史记录账户筛选
        dom.filterAccount.innerHTML = '<option value="">全部</option>';
        accounts.forEach((acc) => {
            const opt = document.createElement('option');
            opt.value = acc.id;
            opt.textContent = acc.username;
            dom.filterAccount.appendChild(opt);
        });
    }

    // =========================================
    // 历史记录
    // =========================================

    // 设置默认日期范围（最近7天）
    function initDateFilters() {
        const today = new Date();
        const weekAgo = new Date();
        weekAgo.setDate(today.getDate() - 7);
        dom.filterEnd.value = formatDate(today);
        dom.filterStart.value = formatDate(weekAgo);
    }

    dom.btnQuery.addEventListener('click', () => loadHistory());

    async function loadHistory() {
        const params = new URLSearchParams();

        if (dom.filterStart.value) params.set('startDate', dom.filterStart.value);
        if (dom.filterEnd.value) params.set('endDate', dom.filterEnd.value);
        if (dom.filterAccount.value) params.set('accountId', dom.filterAccount.value);

        try {
            const resp = await apiRequest(`/api/history?${params.toString()}`);
            if (resp.ok) {
                const data = await resp.json();
                renderHistory(data.records || []);
            }
        } catch (err) {
            showToast('查询历史记录失败', 'error');
        }
    }

    function renderHistory(records) {
        if (records.length === 0) {
            dom.historyTbody.innerHTML = '<tr class="empty-row"><td colspan="4">暂无记录</td></tr>';
            return;
        }

        dom.historyTbody.innerHTML = records
            .map(
                (r) => `
            <tr>
                <td>${escapeHtml(r.username)}</td>
                <td>${escapeHtml(r.startTime)}</td>
                <td>${escapeHtml(r.endTime)}</td>
                <td>${escapeHtml(r.duration)}</td>
            </tr>
        `
            )
            .join('');
    }

    function escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // =========================================
    // 初始化
    // =========================================

    initDateFilters();
    initAuth();
})();
