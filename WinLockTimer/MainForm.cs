namespace WinLockTimer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using WinLockTimer.Data;
using WinLockTimer.Models;
using WinLockTimer.Forms;
using WinLockTimer.Services;

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

    // 标记密码是否已通过验证，允许编辑设置区域
    private bool _settingsUnlocked = false;
    // 抑制设置区域 Enter 事件验证（程序化操作期间不弹验证窗口）
    private bool _suppressSettingsEnter = false;

    // 进程关闭设置
    private bool _killProcessesOnExpiry = false;
    private List<string> _killProcessNames = new();

    // 提醒方式回退用
    private int _previousReminderIndex = 0;

    private int _currentAccountId = -1;
    private DateTime _sessionStartTime;
    private AccountRepository _accountRepo;
    private TimerRecordRepository _recordRepo;

    // 系统托盘
    private NotifyIcon _trayIcon;
    private ContextMenuStrip _trayMenu;
    private bool _forceExit = false;

    // TimerService 引用
    private readonly TimerService _timerService;

    private enum ReminderType
    {
        Popup = 0,          // 弹窗提醒
        Sound = 1,          // 声音提醒（系统提示音，原"语音提醒"）
        PopupSound = 2,     // 弹窗+声音提醒（原"弹窗+语音提醒"）
        Voice = 3,          // 语音播报提醒（播放 wav 文件，仅 10/5/1 分钟）
        PopupVoice = 4      // 弹窗+语音播报提醒
    }

    private bool _allowVisible = true;

    public MainForm(bool startMinimized = false)
    {
        InitializeComponent();

        if (startMinimized)
        {
            _allowVisible = false;
        }

        // 获取 TimerService 单例
        _timerService = TimerService.Instance;

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

        InitializeTrayIcon();

        // 初始化界面状态
        UpdateTimeDisplay(TimeSpan.Zero);
        UpdateStatus("准备设置时间...");

        // 设置按钮初始状态
        pauseButton.Enabled = false;
        resetButton.Enabled = false;

        // 设置提醒方式下拉框事件
        reminderTypeComboBox.SelectedIndexChanged += ReminderTypeComboBox_SelectedIndexChanged;

        // NumericUpDown 清空后失去焦点自动归零
        hoursNumericUpDown.Leave += NumericUpDown_Leave;
        minutesNumericUpDown.Leave += NumericUpDown_Leave;

        // 密码文本框获得焦点时验证家长密码
        passwordTextBox.Enter += SettingsControl_Enter;
        reminderTypeComboBox.Enter += SettingsControl_Enter;

        // 监听 TimerService 状态变更（从 Web API 触发的操作）
        _timerService.StateChanged += OnTimerServiceStateChanged;
        // 监听 TimerService 倒计时过期事件（无论从桌面还是 Web 启动都会触发锁屏）
        _timerService.TimerExpired += OnTimerServiceExpired;
        WindowsSessionService.Instance.StateChanged += OnWindowsSessionStateChanged;

        // 绑定小时和分钟输入框的失去焦点事件，处理为空时设为0
        hoursNumericUpDown.Leave += NumericUpDown_Leave;
        minutesNumericUpDown.Leave += NumericUpDown_Leave;

        // 加载保存的设置
        LoadSavedSettings();
        UpdateWindowsSessionStatus();

        // 数据库初始化
        try 
        {
            DatabaseManager.InitializeDatabase();
            _accountRepo = new AccountRepository();
            _recordRepo = new TimerRecordRepository();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"数据库初始化失败: {ex.Message}\n账户和历史记录功能将无法使用。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// TimerService 状态变更回调（可能从非 UI 线程触发）
    /// 同步 WinForms UI 显示
    /// </summary>
    private void OnTimerServiceStateChanged()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnTimerServiceStateChanged));
            return;
        }

        var status = _timerService.GetStatus();

        // 同步本地状态
        isRunning = status.IsRunning;
        isPaused = status.IsPaused;
        remainingTime = TimeSpan.FromSeconds(status.RemainingSeconds);
        totalTime = TimeSpan.FromSeconds(status.TotalSeconds);
        _currentAccountId = status.CurrentAccountId;
        if (status.IsRunning)
        {
            _sessionStartTime = _timerService.SessionStartTime;
        }

        // 更新 UI
        UpdateTimeDisplay(remainingTime);
        UpdateStatus(status.Status);

        // 更新按钮状态
        if (isRunning)
        {
            _suppressSettingsEnter = true;
            startButton.Enabled = false;
            pauseButton.Enabled = true;
            resetButton.Enabled = true;
            hoursNumericUpDown.Enabled = false;
            minutesNumericUpDown.Enabled = false;
            passwordTextBox.Enabled = false;
            reminderTypeComboBox.Enabled = false;
            killProcessCheckBox.Enabled = false;
            editProcessListButton.Enabled = false;
            accountManagementMenuItem.Enabled = false;
            _suppressSettingsEnter = false;

            if (isPaused)
            {
                pauseButton.Text = "继续";
                pauseButton.BackColor = Color.LightBlue;

                // 暂停时停止本地计时器
                if (timer.Enabled) timer.Stop();
            }
            else
            {
                pauseButton.Text = "暂停";
                pauseButton.BackColor = Color.LightYellow;

                // 运行时确保本地计时器也在运行
                if (!timer.Enabled) timer.Start();
            }
        }
        else
        {
            _suppressSettingsEnter = true;
            startButton.Enabled = true;
            pauseButton.Enabled = false;
            resetButton.Enabled = false;
            pauseButton.Text = "暂停";
            pauseButton.BackColor = Color.LightYellow;
            hoursNumericUpDown.Enabled = true;
            minutesNumericUpDown.Enabled = true;
            passwordTextBox.Enabled = true;
            reminderTypeComboBox.Enabled = true;
            killProcessCheckBox.Enabled = true;
            editProcessListButton.Enabled = killProcessCheckBox.Checked;
            accountManagementMenuItem.Enabled = true;
            _suppressSettingsEnter = false;

            if (timer.Enabled) timer.Stop();
        }
    }

    private void OnWindowsSessionStateChanged()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnWindowsSessionStateChanged));
            return;
        }

        UpdateWindowsSessionStatus();
    }

    /// <summary>
    /// TimerService 倒计时过期回调
    /// 无论倒计时从桌面 GUI 还是 Web UI 启动，到期都执行锁屏
    /// </summary>
    private void OnTimerServiceExpired()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnTimerServiceExpired));
            return;
        }

        // 停止本地计时器
        timer.Stop();

        // 保存使用记录
        SaveTimerRecord();

        isRunning = false;
        isPaused = false;

        // 如果启用了关闭程序功能，先关闭游戏/浏览器
        if (_killProcessesOnExpiry)
        {
            try
            {
                var namesToKill = _killProcessNames.Count > 0
                    ? _killProcessNames
                    : new List<string>(ProcessKillerService.DefaultProcessNames);
                var killed = ProcessKillerService.KillProcesses(namesToKill);
                if (killed.Count > 0)
                {
                    Debug.WriteLine($"已关闭进程: {string.Join(", ", killed)}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"关闭进程失败: {ex.Message}");
            }
        }

        // 执行锁屏
        LockScreen();

        // 重置界面
        remainingTime = TimeSpan.Zero;
        UpdateTimeDisplay(TimeSpan.Zero);
        _suppressSettingsEnter = true;
        startButton.Enabled = true;
        pauseButton.Enabled = false;
        resetButton.Enabled = false;
        pauseButton.Text = "暂停";
        pauseButton.BackColor = Color.LightYellow;
        hoursNumericUpDown.Enabled = true;
        minutesNumericUpDown.Enabled = true;
        passwordTextBox.Enabled = true;
        reminderTypeComboBox.Enabled = true;
        killProcessCheckBox.Enabled = true;
        editProcessListButton.Enabled = killProcessCheckBox.Checked;
        accountManagementMenuItem.Enabled = true;
        _suppressSettingsEnter = false;

        UpdateStatus("准备设置时间...");

        MessageBox.Show(this, "时间到！电脑已锁屏。", "WinLockTimer", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AccountManagementMenuItem_Click(object sender, EventArgs e)
    {
        if (VerifyPassword("打开账户管理"))
        {
            new AccountManagementForm().ShowDialog(this);
        }
    }

    private void HistoryMenuItem_Click(object sender, EventArgs e)
    {
        new HistoryForm().ShowDialog(this);
    }



    /// <summary>
    /// 设置控件获得焦点时，验证家长密码
    /// 如果已有密码且未解锁，需要先验证密码才能编辑
    /// </summary>
    private void SettingsControl_Enter(object? sender, EventArgs e)
    {
        // 程序化操作期间不弹验证窗口
        if (_suppressSettingsEnter) return;

        // 如果没有设置密码，或已经解锁，直接允许编辑
        if (string.IsNullOrEmpty(parentPassword) || _settingsUnlocked)
        {
            return;
        }

        // 需要验证家长密码
        if (VerifyPassword("修改设置"))
        {
            _settingsUnlocked = true; // 验证通过，本次会话内不再重复验证

            // 如果是密码框获得焦点，清空显示以便输入新密码
            if (sender == passwordTextBox)
            {
                passwordTextBox.Text = "";
                passwordTextBox.PasswordChar = '*';
            }
        }
        else
        {
            // 验证失败，移走焦点到其他控件
            // 使用 BeginInvoke 延迟执行，避免在 Enter 事件中直接切换焦点导致问题
            BeginInvoke(new Action(() =>
            {
                // 恢复提醒方式为原来的值
                if (sender == reminderTypeComboBox)
                {
                    reminderTypeComboBox.SelectedIndex = (int)currentReminderType;
                }
                startButton.Focus();
            }));
        }
    }

    private void ReminderTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        // 加载设置时不验证密码
        if (_isLoadingSettings)
        {
            _previousReminderIndex = reminderTypeComboBox.SelectedIndex;
            currentReminderType = (ReminderType)reminderTypeComboBox.SelectedIndex;
            return;
        }

        // 验证家长密码
        if (!VerifyPassword("修改提醒方式"))
        {
            // 回退到之前的选项
            _isLoadingSettings = true;
            reminderTypeComboBox.SelectedIndex = _previousReminderIndex;
            _isLoadingSettings = false;
            return;
        }

        _previousReminderIndex = reminderTypeComboBox.SelectedIndex;
        currentReminderType = (ReminderType)reminderTypeComboBox.SelectedIndex;
    }



    private void NumericUpDown_Leave(object? sender, EventArgs e)
    {
        if (sender is NumericUpDown nud)
        {
            // 在 Leave 时捕获当前文本状态（此时控件内部 ValidateEditText 尚未执行）
            string currentText = nud.Text;
            if (string.IsNullOrWhiteSpace(currentText))
            {
                // 使用 BeginInvoke 延迟执行，确保在控件内部 ValidateEditText 完成后再覆盖
                // ValidateEditText 会在 OnLostFocus 中将文本恢复为旧 Value，
                // 我们需要在那之后强制将值和文本都设为 0
                BeginInvoke(new Action(() =>
                {
                    nud.Value = 0;
                    nud.Text = "0";
                }));
            }
        }
    }

    private void StartButton_Click(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            // 当输入框为空时，将其值设为0
            if (string.IsNullOrWhiteSpace(hoursNumericUpDown.Text))
            {
                hoursNumericUpDown.Value = 0;
            }
            if (string.IsNullOrWhiteSpace(minutesNumericUpDown.Text))
            {
                minutesNumericUpDown.Value = 0;
            }

            // 获取用户设置的时间
            int hours = (int)hoursNumericUpDown.Value;
            int minutes = (int)minutesNumericUpDown.Value;

            if (hours == 0 && minutes == 0)
            {
                MessageBox.Show(this, "请设置有效的时间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 如果设置已解锁（说明用户验证过密码并可能做了修改），保存当前设置
            if (_settingsUnlocked)
            {
                SaveCurrentSettings();
            }

            totalTime = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            remainingTime = totalTime;

            // 重置提醒触发记录
            triggeredReminders.Clear();

            // 账户选择逻辑
            _currentAccountId = -1; // Default
            if (DatabaseManager.IsDatabaseAvailable())
            {
                var accounts = _accountRepo.GetAllAccounts();
                if (accounts.Count > 0)
                {
                    using (var selectionForm = new AccountSelectionForm())
                    {
                        if (selectionForm.ShowDialog(this) == DialogResult.OK)
                        {
                            _currentAccountId = selectionForm.SelectedAccountId;
                        }
                        else
                        {
                            return; // 用户取消选择，取消开始
                        }
                    }
                }
            }

            // 同步到 TimerService（Web API 可见）
            _timerService.Start(hours, minutes, _currentAccountId);

            // 启动计时器
            timer.Start();
            isRunning = true;
            isPaused = false;
            _sessionStartTime = DateTime.Now; // 记录开始时间

            // 更新界面状态
            _suppressSettingsEnter = true;
            startButton.Enabled = false;
            pauseButton.Enabled = true;
            resetButton.Enabled = true;
            hoursNumericUpDown.Enabled = false;
            minutesNumericUpDown.Enabled = false;
            passwordTextBox.Enabled = false;
            reminderTypeComboBox.Enabled = false;
            killProcessCheckBox.Enabled = false;
            editProcessListButton.Enabled = false;
            accountManagementMenuItem.Enabled = false; // 计时中禁用账户管理
            _suppressSettingsEnter = false;

            UpdateStatus("倒计时运行中...");
        }
    }

    private void PauseButton_Click(object sender, EventArgs e)
    {
        if (isRunning)
        {
            if (isPaused)
            {
                // 继续计时 - 不需要验证密码
                timer.Start();
                isPaused = false;
                pauseButton.Text = "暂停";
                pauseButton.BackColor = Color.LightYellow;
                UpdateStatus("倒计时运行中...");

                // 同步到 TimerService
                _timerService.Resume();
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

                    // 同步到 TimerService
                    _timerService.Pause();
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
        
        if (isRunning) // Only save if it was running
        {
             SaveTimerRecord();
        }

        isRunning = false;
        isPaused = false;

        // 重置界面状态
        remainingTime = TimeSpan.Zero;
        UpdateTimeDisplay(TimeSpan.Zero);

        _suppressSettingsEnter = true;
        startButton.Enabled = true;
        pauseButton.Enabled = false;
        resetButton.Enabled = false;
        pauseButton.Text = "暂停";
        pauseButton.BackColor = Color.LightYellow;
        hoursNumericUpDown.Enabled = true;
        minutesNumericUpDown.Enabled = true;
        passwordTextBox.Enabled = true;
        reminderTypeComboBox.Enabled = true;
        killProcessCheckBox.Enabled = true;
        editProcessListButton.Enabled = killProcessCheckBox.Checked;
        accountManagementMenuItem.Enabled = true; // 重置后启用账户管理
        _suppressSettingsEnter = false;

        // 同步到 TimerService
        _timerService.Reset();

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
            if (passwordForm.ShowDialog(this) == DialogResult.OK)
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
                        MessageBox.Show(this, "密码错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show(this, "密码错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
        }
        return false;
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        // 统一通过 TimerService.Tick() 驱动倒计时递减
        // TimerService 内部会在过期时触发 TimerExpired 事件，
        // 由 OnTimerServiceExpired 处理锁屏逻辑
        _timerService.Tick();

        // 从 TimerService 同步状态到本地
        var status = _timerService.GetStatus();
        remainingTime = TimeSpan.FromSeconds(status.RemainingSeconds);
        totalTime = TimeSpan.FromSeconds(status.TotalSeconds);
        isRunning = status.IsRunning;
        isPaused = status.IsPaused;
        _currentAccountId = status.CurrentAccountId;
        if (status.IsRunning)
        {
            _sessionStartTime = _timerService.SessionStartTime;
        }

        UpdateTimeDisplay(remainingTime);

        // 检查是否需要触发提醒
        CheckReminders();

        // 更新状态显示
        if (isRunning)
        {
            UpdateStatus($"剩余时间: {FormatTimeSpan(remainingTime)}");
        }
    }

    private void SaveTimerRecord()
    {
        try
        {
            if (!DatabaseManager.IsDatabaseAvailable()) return;
            
            double duration = (DateTime.Now - _sessionStartTime).TotalSeconds;
            if (duration < 1) return; // Too short

            var record = new TimerRecord
            {
                AccountId = _currentAccountId,
                StartTime = _sessionStartTime,
                EndTime = DateTime.Now,
                DurationSeconds = duration
            };
            
            _recordRepo.AddRecord(record);
        }
        catch (Exception ex)
        {
            // Fail silently or log
            Debug.WriteLine(ex.Message);
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
                MessageBox.Show(this, message, "WinLockTimer 提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;
            case ReminderType.Sound:
                PlaySoundReminder();
                break;
            case ReminderType.PopupSound:
                MessageBox.Show(this, message, "WinLockTimer 提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PlaySoundReminder();
                break;
            case ReminderType.Voice:
                PlayVoiceReminder(reminderTime);
                break;
            case ReminderType.PopupVoice:
                PlayVoiceReminder(reminderTime);
                MessageBox.Show(this, message, "WinLockTimer 提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;
        }
    }

    // 语音提醒音频文件映射（分钟 -> 文件名）
    private static readonly Dictionary<int, string> VoiceReminderFiles = new()
    {
        { 10, "remaining_10_minutes.wav" },
        { 5, "remaining_5_minutes.wav" },
        { 1, "remaining_1_minute.wav" }
    };

    /// <summary>
    /// 声音提醒：播放系统提示音（所有提醒时间点都会触发）
    /// </summary>
    private void PlaySoundReminder()
    {
        try
        {
            SystemSounds.Exclamation.Play();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"声音提醒播放失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 语音播报提醒：播放 sounds 目录下的 wav 文件（仅 10/5/1 分钟有对应文件）
    /// </summary>
    private void PlayVoiceReminder(TimeSpan reminderTime)
    {
        try
        {
            int minutes = (int)reminderTime.TotalMinutes;
            if (VoiceReminderFiles.TryGetValue(minutes, out string? fileName))
            {
                // 定位 sounds 目录下的 wav 文件
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sounds", fileName);
                if (File.Exists(soundPath))
                {
                    var player = new SoundPlayer(soundPath);
                    player.Play(); // 异步播放，不阻塞UI
                    return;
                }
                Debug.WriteLine($"语音文件不存在: {soundPath}");
            }
            // 当前时间点没有对应的语音文件（4/3/2分钟），不播放任何声音
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"语音播报播放失败: {ex.Message}");
        }
    }

    // 标志位：加载设置时不触发密码验证
    private bool _isLoadingSettings = false;

    private void KillProcessCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        // 加载设置时不需要验证密码
        if (_isLoadingSettings) return;

        // 验证家长密码
        if (!VerifyPassword("修改关闭程序设置"))
        {
            // 密码验证失败，回退 CheckBox 状态
            _isLoadingSettings = true; // 防止递归触发
            killProcessCheckBox.Checked = _killProcessesOnExpiry;
            _isLoadingSettings = false;
            return;
        }

        _settingsUnlocked = true; // 标记设置已修改，确保后续保存
        _killProcessesOnExpiry = killProcessCheckBox.Checked;
        editProcessListButton.Enabled = killProcessCheckBox.Checked;
    }

    private void EditProcessListButton_Click(object sender, EventArgs e)
    {
        // 验证家长密码
        if (!VerifyPassword("编辑关闭程序列表"))
        {
            return;
        }

        var currentNames = _killProcessNames.Count > 0
            ? _killProcessNames
            : new List<string>(ProcessKillerService.DefaultProcessNames);

        using (var form = new ProcessListEditForm(currentNames))
        {
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _killProcessNames = form.ProcessNames;
                _settingsUnlocked = true; // 标记设置已修改，确保后续保存
            }
        }
    }

    private void LockScreen()
    {
        try
        {
            // 使用Windows系统命令锁屏
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
            WindowsSessionService.Instance.MarkLocked();
            UpdateWindowsSessionStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"锁屏失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    private void UpdateWindowsSessionStatus()
    {
        var sessionService = WindowsSessionService.Instance;
        windowsSessionStatusLabel.Text = $"Windows: {sessionService.StateText}";
        windowsSessionStatusLabel.ForeColor = sessionService.IsLocked ? Color.Firebrick : Color.DimGray;
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

            // 设置标志位避免触发密码验证
            _isLoadingSettings = true;

            // 加载提醒方式
            if (settings.ReminderType >= 0 && settings.ReminderType < reminderTypeComboBox.Items.Count)
            {
                reminderTypeComboBox.SelectedIndex = settings.ReminderType;
                currentReminderType = (ReminderType)settings.ReminderType;
                _previousReminderIndex = settings.ReminderType;
            }

            // 加载关闭程序设置
            _killProcessesOnExpiry = settings.KillProcessesOnExpiry;
            killProcessCheckBox.Checked = settings.KillProcessesOnExpiry;
            editProcessListButton.Enabled = settings.KillProcessesOnExpiry;
            _killProcessNames = ProcessKillerService.DeserializeProcessNames(settings.KillProcessNames);

            _isLoadingSettings = false;
        }
        catch (Exception ex)
        {
            // 如果加载设置失败，使用默认设置
            MessageBox.Show(this, $"加载设置失败: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // 确定要保存的密码值
            string passwordToSave;

            if (currentPassword == "●●●●●●")
            {
                // 占位符未被修改，保留原密码哈希，仅保存其他设置（如提醒方式、关闭程序等）
                // 使用 SaveSettingsPreservePassword 保留原密码
                var settings = SettingsManager.LoadSettings();
                settings.ReminderType = reminderTypeComboBox.SelectedIndex;
                settings.RememberSettings = true;
                settings.KillProcessesOnExpiry = _killProcessesOnExpiry;
                settings.KillProcessNames = ProcessKillerService.SerializeProcessNames(_killProcessNames);

                // 直接序列化保存，不触发密码重新哈希
                SettingsManager.SaveSettingsRaw(settings);

                // 重置解锁状态
                _settingsUnlocked = false;
                return;
            }
            else if (string.IsNullOrEmpty(currentPassword))
            {
                // 用户清空了密码
                passwordToSave = "";
            }
            else
            {
                // 用户输入了新密码
                passwordToSave = currentPassword;
            }

            var newSettings = new SettingsManager.AppSettings
            {
                ParentPassword = passwordToSave,
                ReminderType = reminderTypeComboBox.SelectedIndex,
                RememberSettings = true,
                KillProcessesOnExpiry = _killProcessesOnExpiry,
                KillProcessNames = ProcessKillerService.SerializeProcessNames(_killProcessNames)
            };

            SettingsManager.SaveSettings(newSettings);

            // 更新内存中的密码
            var loadedSettings = SettingsManager.LoadSettings();
            parentPassword = loadedSettings.ParentPassword;

            // 如果密码被清空，重置UI状态
            if (string.IsNullOrEmpty(passwordToSave))
            {
                passwordTextBox.PasswordChar = '*';
            }
            else
            {
                // 密码已保存，重新显示占位符
                passwordTextBox.Text = "●●●●●●";
                passwordTextBox.PasswordChar = '●';
            }

            // 重置解锁状态
            _settingsUnlocked = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"保存设置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeTrayIcon()
    {
        _trayMenu = new ContextMenuStrip();
        var showItem = new ToolStripMenuItem("显示主界面");
        var exitItem = new ToolStripMenuItem("完全退出");

        showItem.Click += (s, e) => { 
            _allowVisible = true;
            this.Show(); 
            this.WindowState = FormWindowState.Normal; 
            this.Activate(); 
        };
        exitItem.Click += TrayExit_Click;

        _trayMenu.Items.Add(showItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon();
        _trayIcon.Text = "WinLockTimer";
        _trayIcon.Icon = this.Icon ?? SystemIcons.Application;
        _trayIcon.ContextMenuStrip = _trayMenu;
        _trayIcon.Visible = true;

        _trayIcon.DoubleClick += (s, e) => { 
            _allowVisible = true;
            this.Show(); 
            this.WindowState = FormWindowState.Normal; 
            this.Activate(); 
        };
    }

    protected override void SetVisibleCore(bool value)
    {
        if (!_allowVisible)
        {
            value = false;
            if (!this.IsHandleCreated) CreateHandle();
        }
        base.SetVisibleCore(value);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == Program.ShowMainWindowMessageId)
        {
            ShowMainWindowFromSecondInstance();
            return;
        }

        base.WndProc(ref m);
    }

    private void ShowMainWindowFromSecondInstance()
    {
        _allowVisible = true;
        Show();
        WindowState = FormWindowState.Normal;
        ShowInTaskbar = true;
        BringToFront();
        Activate();
    }

    private void TrayExit_Click(object? sender, EventArgs e)
    {
        if (!VerifyPassword("退出程序")) return;
        
        _forceExit = true;
        this.Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // 如果是用户点击窗口的关闭按钮（X），且不是托盘菜单强制退出，则缩放到系统托盘
        if (e.CloseReason == CloseReason.UserClosing && !_forceExit)
        {
            e.Cancel = true;
            this.Hide();
            _trayIcon.ShowBalloonTip(2000, "WinLockTimer", "程序正在后台运行，双击图标恢复", ToolTipIcon.Info);
            return;
        }

        // 如果是强制退出，且倒计时正在运行中，给最后一次确认提示（密码已经验证过了）
        if (_forceExit && isRunning)
        {
            var result = MessageBox.Show(this,
                "倒计时仍在运行中，确定要完全退出吗？\n(退出后将无法自动锁定屏幕)",
                "确认退出",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                _forceExit = false;
                return;
            }
        }

        // 清理系统托盘图标
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        // 取消 TimerService 事件订阅
        _timerService.StateChanged -= OnTimerServiceStateChanged;
        _timerService.TimerExpired -= OnTimerServiceExpired;
        WindowsSessionService.Instance.StateChanged -= OnWindowsSessionStateChanged;

        // 如果设置已解锁（用户验证过密码并可能做了修改），保存当前设置
        if (_settingsUnlocked)
        {
            SaveCurrentSettings();
        }

        base.OnFormClosing(e);
    }

    private void ClearSettingsButton_Click(object sender, EventArgs e)
    {
        if (isRunning)
        {
            MessageBox.Show(this, "倒计时运行中，无法清除设置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        var result = MessageBox.Show(this,
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
                _isLoadingSettings = true;
                reminderTypeComboBox.SelectedIndex = 0;
                currentReminderType = ReminderType.Popup;
                _settingsUnlocked = false; // 重置解锁状态
                _previousReminderIndex = 0;
                killProcessCheckBox.Checked = false;
                _isLoadingSettings = false;
                _killProcessesOnExpiry = false;
                editProcessListButton.Enabled = false;
                _killProcessNames.Clear();

                MessageBox.Show(this, "设置已成功清除！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"清除设置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
