namespace WinLockTimer.Services;

using System;
using System.Collections.Generic;

/// <summary>
/// 倒计时业务逻辑服务（单例）
/// 供 WinForms UI 和 Web API 共同访问
/// </summary>
public class TimerService
{
    private static readonly Lazy<TimerService> _instance = new(() => new TimerService());
    public static TimerService Instance => _instance.Value;

    private readonly object _lock = new();

    // 倒计时状态
    private TimeSpan _remainingTime;
    private TimeSpan _totalTime;
    private bool _isRunning;
    private bool _isPaused;
    private int _currentAccountId = -1;
    private DateTime _sessionStartTime;

    // 提醒相关
    private readonly List<TimeSpan> _reminderTimes;
    private readonly HashSet<TimeSpan> _triggeredReminders;

    // 状态变更事件（UI 线程安全地更新界面）
    public event Action? StateChanged;
    public event Action<TimeSpan>? ReminderTriggered;
    public event Action? TimerExpired;

    private TimerService()
    {
        _reminderTimes = new List<TimeSpan>
        {
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(4),
            TimeSpan.FromMinutes(3),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(1)
        };
        _triggeredReminders = new HashSet<TimeSpan>();
    }

    #region 属性

    public TimeSpan RemainingTime
    {
        get { lock (_lock) return _remainingTime; }
    }

    public TimeSpan TotalTime
    {
        get { lock (_lock) return _totalTime; }
    }

    public bool IsRunning
    {
        get { lock (_lock) return _isRunning; }
    }

    public bool IsPaused
    {
        get { lock (_lock) return _isPaused; }
    }

    public int CurrentAccountId
    {
        get { lock (_lock) return _currentAccountId; }
        set { lock (_lock) _currentAccountId = value; }
    }

    public DateTime SessionStartTime
    {
        get { lock (_lock) return _sessionStartTime; }
    }

    #endregion

    #region 控制方法

    /// <summary>
    /// 启动倒计时
    /// </summary>
    public bool Start(int hours, int minutes, int accountId = -1)
    {
        lock (_lock)
        {
            if (_isRunning) return false;
            if (hours == 0 && minutes == 0) return false;

            _totalTime = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            _remainingTime = _totalTime;
            _triggeredReminders.Clear();
            _currentAccountId = accountId;
            _sessionStartTime = DateTime.Now;
            _isRunning = true;
            _isPaused = false;
        }

        StateChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 暂停倒计时
    /// </summary>
    public bool Pause()
    {
        lock (_lock)
        {
            if (!_isRunning || _isPaused) return false;
            _isPaused = true;
        }

        StateChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 继续倒计时
    /// </summary>
    public bool Resume()
    {
        lock (_lock)
        {
            if (!_isRunning || !_isPaused) return false;
            _isPaused = false;
        }

        StateChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 重置倒计时
    /// </summary>
    public bool Reset()
    {
        lock (_lock)
        {
            _isRunning = false;
            _isPaused = false;
            _remainingTime = TimeSpan.Zero;
        }

        StateChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 每秒 Tick，由 WinForms Timer 调用
    /// </summary>
    public void Tick()
    {
        bool expired = false;
        TimeSpan? reminderToTrigger = null;

        lock (_lock)
        {
            if (!_isRunning || _isPaused) return;

            if (_remainingTime > TimeSpan.Zero)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));

                // 检查提醒
                foreach (var rt in _reminderTimes)
                {
                    if (rt < _totalTime && _remainingTime <= rt && !_triggeredReminders.Contains(rt))
                    {
                        _triggeredReminders.Add(rt);
                        reminderToTrigger = rt;
                        break;
                    }
                }

                if (_remainingTime <= TimeSpan.Zero)
                {
                    _isRunning = false;
                    expired = true;
                }
            }
        }

        // 在锁外触发事件，避免死锁
        StateChanged?.Invoke();

        if (reminderToTrigger.HasValue)
        {
            ReminderTriggered?.Invoke(reminderToTrigger.Value);
        }

        if (expired)
        {
            TimerExpired?.Invoke();
        }
    }

    #endregion

    #region 状态查询

    /// <summary>
    /// 获取当前倒计时状态（供 Web API 使用）
    /// </summary>
    public TimerStatus GetStatus()
    {
        lock (_lock)
        {
            string statusText;
            if (!_isRunning && _remainingTime <= TimeSpan.Zero)
                statusText = "准备设置时间...";
            else if (_isPaused)
                statusText = "倒计时已暂停";
            else
                statusText = $"剩余时间: {FormatTimeSpan(_remainingTime)}";

            return new TimerStatus
            {
                IsRunning = _isRunning,
                IsPaused = _isPaused,
                RemainingSeconds = (int)_remainingTime.TotalSeconds,
                TotalSeconds = (int)_totalTime.TotalSeconds,
                RemainingDisplay = FormatTimeSpan(_remainingTime),
                Status = statusText,
                CurrentAccountId = _currentAccountId
            };
        }
    }

    private static string FormatTimeSpan(TimeSpan time)
    {
        return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
    }

    #endregion
}

/// <summary>
/// 倒计时状态 DTO
/// </summary>
public class TimerStatus
{
    public bool IsRunning { get; set; }
    public bool IsPaused { get; set; }
    public int RemainingSeconds { get; set; }
    public int TotalSeconds { get; set; }
    public string RemainingDisplay { get; set; } = "00:00:00";
    public string Status { get; set; } = "";
    public int CurrentAccountId { get; set; } = -1;
}
