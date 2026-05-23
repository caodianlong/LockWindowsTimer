namespace WinLockTimer.Services;

using System;
using Microsoft.Win32;

/// <summary>
/// Tracks the current Windows session lock state for desktop and Web UI display.
/// </summary>
public sealed class WindowsSessionService
{
    private static readonly Lazy<WindowsSessionService> _instance = new(() => new WindowsSessionService());
    public static WindowsSessionService Instance => _instance.Value;

    private readonly object _lock = new();
    private bool _isLocked;
    private DateTime _lastChangedAt = DateTime.Now;

    public event Action? StateChanged;

    private WindowsSessionService()
    {
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    public bool IsLocked
    {
        get { lock (_lock) return _isLocked; }
    }

    public DateTime LastChangedAt
    {
        get { lock (_lock) return _lastChangedAt; }
    }

    public string StateText => IsLocked ? "已锁屏" : "已解锁";

    public void MarkLocked()
    {
        SetLocked(true);
    }

    public void MarkUnlocked()
    {
        SetLocked(false);
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionLock)
        {
            SetLocked(true);
        }
        else if (e.Reason == SessionSwitchReason.SessionUnlock)
        {
            SetLocked(false);
        }
    }

    private void SetLocked(bool isLocked)
    {
        var changed = false;
        lock (_lock)
        {
            if (_isLocked == isLocked) return;

            _isLocked = isLocked;
            _lastChangedAt = DateTime.Now;
            changed = true;
        }

        if (changed)
        {
            StateChanged?.Invoke();
        }
    }
}
