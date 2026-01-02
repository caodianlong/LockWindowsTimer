namespace WinLockTimer.Models;

using System;

public class TimerRecord
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DurationSeconds { get; set; }
}
