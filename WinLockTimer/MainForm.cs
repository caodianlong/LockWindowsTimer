namespace WinLockTimer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private TimeSpan remainingTime;
    private TimeSpan totalTime;
    private bool isRunning = false;
    private bool isPaused = false;
    private readonly List<TimeSpan> reminderTimes;
    private readonly HashSet<TimeSpan> triggeredReminders;
    private string parentPassword = "";
    private ReminderType currentReminderType = ReminderType.Popup;

    private enum ReminderType
    {
        Popup = 0,
        Voice = 1,
        Both = 2
    }

    public MainForm()
    {
        InitializeComponent();

        // 初始化提醒时间点：10, 5, 4, 3, 2, 1分钟
        reminderTimes = new List<TimeSpan>
        {
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(4),
            TimeSpan.FromMinutes(3),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(1)
        };

        triggeredReminders = new HashSet<TimeSpan>();

        // 初始化界面状态
        UpdateTimeDisplay(TimeSpan.Zero);
        UpdateStatus("准备设置时间...");

        // 设置按钮初始状态
        pauseButton.Enabled = false;
        resetButton.Enabled = false;

        // 设置提醒方式下拉框事件
        reminderTypeComboBox.SelectedIndexChanged += ReminderTypeComboBox_SelectedIndexChanged;

        // 加载保存的设置
        LoadSavedSettings();
    }

    private void ReminderTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        currentReminderType = (ReminderType)reminderTypeComboBox.SelectedIndex;
    }

    private void StartButton_Click(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            // 获取用户设置的时间
            int hours = (int)hoursNumericUpDown.Value;
            int minutes = (int)minutesNumericUpDown.Value;

            if (hours == 0 && minutes == 0)
            {
                MessageBox.Show("请设置有效的时间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查密码设置是否有变化，如果有变化则保存
            string currentPassword = passwordTextBox.Text.Trim();
            if (currentPassword != "●●●●●●" && !string.IsNullOrEmpty(currentPassword))
            {
                // 用户输入了新密码，需要保存
                SaveCurrentSettings();
            }

            totalTime = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            remainingTime = totalTime;

            // 重置提醒触发记录
            triggeredReminders.Clear();

            // 启动计时器
            timer.Start();
            isRunning = true;
            isPaused = false;

            // 更新界面状态
            startButton.Enabled = false;
            pauseButton.Enabled = true;
            resetButton.Enabled = true;
            hoursNumericUpDown.Enabled = false;
            minutesNumericUpDown.Enabled = false;
            passwordTextBox.Enabled = false;
            reminderTypeComboBox.Enabled = false;

            UpdateStatus("倒计时运行中...");
        }
    }

    private void PauseButton_Click(object sender, EventArgs e)
    {
        if (isRunning)
        {
            if (isPaused)
            {
                // 继续计时
                if (VerifyPassword("继续倒计时"))
                {
                    timer.Start();
                    isPaused = false;
                    pauseButton.Text = "暂停";
                    pauseButton.BackColor = Color.LightYellow;
                    UpdateStatus("倒计时运行中...");
                }
            }
            else
            {
                // 暂停计时
                if (VerifyPassword("暂停倒计时"))
                {
                    timer.Stop();
                    isPaused = true;
                    pauseButton.Text = "继续";
                    pauseButton.BackColor = Color.LightBlue;
                    UpdateStatus("倒计时已暂停");
                }
            }
        }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        if (isRunning && !VerifyPassword("重置倒计时"))
        {
            return;
        }

        // 停止计时器
        timer.Stop();
        isRunning = false;
        isPaused = false;

        // 重置界面状态
        remainingTime = TimeSpan.Zero;
        UpdateTimeDisplay(TimeSpan.Zero);

        startButton.Enabled = true;
        pauseButton.Enabled = false;
        resetButton.Enabled = false;
        pauseButton.Text = "暂停";
        pauseButton.BackColor = Color.LightYellow;
        hoursNumericUpDown.Enabled = true;
        minutesNumericUpDown.Enabled = true;
        passwordTextBox.Enabled = true;
        reminderTypeComboBox.Enabled = true;

        UpdateStatus("准备设置时间...");
    }

    private bool VerifyPassword(string action)
    {
        if (string.IsNullOrEmpty(parentPassword))
        {
            return true; // 如果没有设置密码，直接允许
        }

        using (var passwordForm = new PasswordForm(action))
        {
            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                // 检查parentPassword是否为BCrypt哈希格式
                if (parentPassword.StartsWith("$2") && parentPassword.Length >= 59)
                {
                    // 使用BCrypt验证密码
                    if (BCrypt.Net.BCrypt.Verify(passwordForm.Password, parentPassword))
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("密码错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    // 如果不是BCrypt格式，使用旧方式直接比较
                    if (passwordForm.Password == parentPassword)
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("密码错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
        }
        return false;
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (remainingTime > TimeSpan.Zero)
        {
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            UpdateTimeDisplay(remainingTime);

            // 检查是否需要触发提醒
            CheckReminders();

            // 更新状态显示
            UpdateStatus($"剩余时间: {FormatTimeSpan(remainingTime)}");
        }
        else
        {
            // 倒计时结束
            timer.Stop();
            isRunning = false;

            // 执行锁屏
            LockScreen();

            // 重置界面
            ResetButton_Click(sender, e);

            MessageBox.Show("时间到！电脑已锁屏。", "WinLockTimer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void CheckReminders()
    {
        foreach (var reminderTime in reminderTimes)
        {
            // 只有当提醒时间小于总时间且剩余时间小于等于提醒时间时才触发
            if (reminderTime < totalTime && remainingTime <= reminderTime && !triggeredReminders.Contains(reminderTime))
            {
                // 触发提醒
                triggeredReminders.Add(reminderTime);
                ShowReminder(reminderTime);
                break; // 一次只显示一个提醒
            }
        }
    }

    private void ShowReminder(TimeSpan reminderTime)
    {
        string message;
        if (reminderTime == TimeSpan.FromMinutes(1))
        {
            message = "⚠️ 最后1分钟！请准备保存工作！";
        }
        else
        {
            message = $"⏰ 提醒：还有 {(int)reminderTime.TotalMinutes} 分钟将自动锁屏！";
        }

        // 根据选择的提醒方式执行
        switch (currentReminderType)
        {
            case ReminderType.Popup:
                MessageBox.Show(message, "WinLockTimer 提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;
            case ReminderType.Voice:
                PlayVoiceReminder(reminderTime);
                break;
            case ReminderType.Both:
                MessageBox.Show(message, "WinLockTimer 提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PlayVoiceReminder(reminderTime);
                break;
        }
    }

    private void PlayVoiceReminder(TimeSpan reminderTime)
    {
        try
        {
            // 使用系统提示音作为语音提醒
            SystemSounds.Exclamation.Play();
        }
        catch (Exception ex)
        {
            // 如果语音提醒失败，使用弹窗提醒
            string message = reminderTime == TimeSpan.FromMinutes(1)
                ? "⚠️ 最后1分钟！请准备保存工作！"
                : $"⏰ 提醒：还有 {(int)reminderTime.TotalMinutes} 分钟将自动锁屏！";

            MessageBox.Show(message, "WinLockTimer 提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LockScreen()
    {
        try
        {
            // 使用Windows系统命令锁屏
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"锁屏失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateTimeDisplay(TimeSpan time)
    {
        timeDisplayLabel.Text = FormatTimeSpan(time);

        // 根据剩余时间改变颜色
        if (time.TotalMinutes <= 5)
        {
            timeDisplayLabel.ForeColor = Color.Red;
        }
        else if (time.TotalMinutes <= 10)
        {
            timeDisplayLabel.ForeColor = Color.Orange;
        }
        else
        {
            timeDisplayLabel.ForeColor = Color.DarkBlue;
        }
    }

    private void UpdateStatus(string status)
    {
        statusLabel.Text = status;
    }

    private string FormatTimeSpan(TimeSpan time)
    {
        return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
    }

    /// <summary>
    /// 加载保存的设置
    /// </summary>
    private void LoadSavedSettings()
    {
        try
        {
            var settings = SettingsManager.LoadSettings();

            // 加载密码
            // SettingsManager返回的是BCrypt哈希密码，不能显示在文本框中
            // 在文本框中显示占位符，表示已设置密码
            if (!string.IsNullOrEmpty(settings.ParentPassword))
            {
                passwordTextBox.Text = "●●●●●●"; // 显示占位符，表示密码已设置
                passwordTextBox.PasswordChar = '●'; // 使用圆点显示
            }
            else
            {
                passwordTextBox.Text = ""; // 没有密码时显示为空
                passwordTextBox.PasswordChar = '*'; // 恢复星号显示
            }

            parentPassword = settings.ParentPassword; // 保存哈希密码用于验证

            // 加载提醒方式
            if (settings.ReminderType >= 0 && settings.ReminderType < reminderTypeComboBox.Items.Count)
            {
                reminderTypeComboBox.SelectedIndex = settings.ReminderType;
                currentReminderType = (ReminderType)settings.ReminderType;
            }
        }
        catch (Exception ex)
        {
            // 如果加载设置失败，使用默认设置
            MessageBox.Show($"加载设置失败: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>
    /// 保存当前设置
    /// </summary>
    private void SaveCurrentSettings()
    {
        try
        {
            // 获取当前密码设置
            string currentPassword = passwordTextBox.Text.Trim();

            // 如果用户输入了新密码（不是占位符），则保存新密码
            // 如果显示的是占位符，说明用户没有修改密码，不需要重新保存
            bool shouldSavePassword = false;
            string passwordToSave = "";

            if (currentPassword == "●●●●●●")
            {
                // 用户没有修改密码，不需要重新保存密码设置
                // 直接返回，避免重复保存
                return;
            }
            else if (string.IsNullOrEmpty(currentPassword))
            {
                // 用户清空了密码
                shouldSavePassword = true;
                passwordToSave = "";
            }
            else
            {
                // 用户输入了新密码，需要保存
                shouldSavePassword = true;
                passwordToSave = currentPassword;
            }

            // 只有当密码有变化时才保存设置
            if (shouldSavePassword)
            {
                var settings = new SettingsManager.AppSettings
                {
                    ParentPassword = passwordToSave,
                    ReminderType = reminderTypeComboBox.SelectedIndex,
                    RememberSettings = true
                };

                SettingsManager.SaveSettings(settings);

                // 更新内存中的密码
                var loadedSettings = SettingsManager.LoadSettings();
                parentPassword = loadedSettings.ParentPassword;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存设置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (isRunning)
        {
            if (!VerifyPassword("关闭程序"))
            {
                e.Cancel = true;
                return;
            }

            var result = MessageBox.Show(
                "倒计时仍在运行中，确定要退出吗？",
                "确认退出",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        // 保存当前设置
        SaveCurrentSettings();

        base.OnFormClosing(e);
    }

    private void ClearSettingsButton_Click(object sender, EventArgs e)
    {
        if (isRunning)
        {
            MessageBox.Show("倒计时运行中，无法清除设置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 如果已设置家长密码，需要验证密码
        if (!string.IsNullOrEmpty(parentPassword))
        {
            if (!VerifyPassword("清除设置"))
            {
                return; // 密码验证失败，取消清除操作
            }
        }

        var result = MessageBox.Show(
            "确定要清除所有保存的设置吗？\n这将删除保存的密码和提醒方式设置。",
            "确认清除设置",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            try
            {
                // 删除设置文件
                SettingsManager.DeleteSettings();

                // 重置界面设置
                passwordTextBox.Text = "";
                passwordTextBox.PasswordChar = '*'; // 恢复星号显示
                parentPassword = "";
                reminderTypeComboBox.SelectedIndex = 0;
                currentReminderType = ReminderType.Popup;

                MessageBox.Show("设置已成功清除！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清除设置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
