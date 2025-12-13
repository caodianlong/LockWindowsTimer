namespace WinLockTimer;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        this.timeDisplayLabel = new System.Windows.Forms.Label();
        this.hoursNumericUpDown = new System.Windows.Forms.NumericUpDown();
        this.minutesNumericUpDown = new System.Windows.Forms.NumericUpDown();
        this.hoursLabel = new System.Windows.Forms.Label();
        this.minutesLabel = new System.Windows.Forms.Label();
        this.startButton = new System.Windows.Forms.Button();
        this.pauseButton = new System.Windows.Forms.Button();
        this.resetButton = new System.Windows.Forms.Button();
        this.statusLabel = new System.Windows.Forms.Label();
        this.timer = new System.Windows.Forms.Timer(this.components);
        this.titleLabel = new System.Windows.Forms.Label();
        this.timeSettingGroupBox = new System.Windows.Forms.GroupBox();
        this.controlGroupBox = new System.Windows.Forms.GroupBox();
        this.statusGroupBox = new System.Windows.Forms.GroupBox();
        this.settingsGroupBox = new System.Windows.Forms.GroupBox();
        this.clearSettingsButton = new System.Windows.Forms.Button();
        this.reminderTypeComboBox = new System.Windows.Forms.ComboBox();
        this.reminderTypeLabel = new System.Windows.Forms.Label();
        this.passwordTextBox = new System.Windows.Forms.TextBox();
        this.passwordLabel = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.hoursNumericUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.minutesNumericUpDown)).BeginInit();
        this.timeSettingGroupBox.SuspendLayout();
        this.controlGroupBox.SuspendLayout();
        this.statusGroupBox.SuspendLayout();
        this.settingsGroupBox.SuspendLayout();
        this.SuspendLayout();
        //
        // timeDisplayLabel
        //
        this.timeDisplayLabel.AutoSize = true;
        this.timeDisplayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.timeDisplayLabel.ForeColor = System.Drawing.Color.DarkBlue;
        this.timeDisplayLabel.Location = new System.Drawing.Point(50, 60);
        this.timeDisplayLabel.Name = "timeDisplayLabel";
        this.timeDisplayLabel.Size = new System.Drawing.Size(300, 55);
        this.timeDisplayLabel.TabIndex = 0;
        this.timeDisplayLabel.Text = "00:00:00";
        this.timeDisplayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // hoursNumericUpDown
        //
        this.hoursNumericUpDown.Location = new System.Drawing.Point(20, 30);
        this.hoursNumericUpDown.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
        this.hoursNumericUpDown.Name = "hoursNumericUpDown";
        this.hoursNumericUpDown.Size = new System.Drawing.Size(60, 23);
        this.hoursNumericUpDown.TabIndex = 1;
        this.hoursNumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
        //
        // minutesNumericUpDown
        //
        this.minutesNumericUpDown.Location = new System.Drawing.Point(120, 30);
        this.minutesNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
        this.minutesNumericUpDown.Name = "minutesNumericUpDown";
        this.minutesNumericUpDown.Size = new System.Drawing.Size(60, 23);
        this.minutesNumericUpDown.TabIndex = 2;
        this.minutesNumericUpDown.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
        //
        // hoursLabel
        //
        this.hoursLabel.AutoSize = true;
        this.hoursLabel.Location = new System.Drawing.Point(20, 60);
        this.hoursLabel.Name = "hoursLabel";
        this.hoursLabel.Size = new System.Drawing.Size(35, 15);
        this.hoursLabel.TabIndex = 3;
        this.hoursLabel.Text = "小时";
        //
        // minutesLabel
        //
        this.minutesLabel.AutoSize = true;
        this.minutesLabel.Location = new System.Drawing.Point(120, 60);
        this.minutesLabel.Name = "minutesLabel";
        this.minutesLabel.Size = new System.Drawing.Size(35, 15);
        this.minutesLabel.TabIndex = 4;
        this.minutesLabel.Text = "分钟";
        //
        // startButton
        //
        this.startButton.BackColor = System.Drawing.Color.LightGreen;
        this.startButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.startButton.Location = new System.Drawing.Point(20, 30);
        this.startButton.Name = "startButton";
        this.startButton.Size = new System.Drawing.Size(80, 35);
        this.startButton.TabIndex = 5;
        this.startButton.Text = "开始";
        this.startButton.UseVisualStyleBackColor = false;
        this.startButton.Click += new System.EventHandler(this.StartButton_Click);
        //
        // pauseButton
        //
        this.pauseButton.BackColor = System.Drawing.Color.LightYellow;
        this.pauseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.pauseButton.Location = new System.Drawing.Point(110, 30);
        this.pauseButton.Name = "pauseButton";
        this.pauseButton.Size = new System.Drawing.Size(80, 35);
        this.pauseButton.TabIndex = 6;
        this.pauseButton.Text = "暂停";
        this.pauseButton.UseVisualStyleBackColor = false;
        this.pauseButton.Click += new System.EventHandler(this.PauseButton_Click);
        //
        // resetButton
        //
        this.resetButton.BackColor = System.Drawing.Color.LightCoral;
        this.resetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.resetButton.Location = new System.Drawing.Point(200, 30);
        this.resetButton.Name = "resetButton";
        this.resetButton.Size = new System.Drawing.Size(80, 35);
        this.resetButton.TabIndex = 7;
        this.resetButton.Text = "重置";
        this.resetButton.UseVisualStyleBackColor = false;
        this.resetButton.Click += new System.EventHandler(this.ResetButton_Click);
        //
        // statusLabel
        //
        this.statusLabel.AutoSize = true;
        this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.statusLabel.ForeColor = System.Drawing.Color.DarkGreen;
        this.statusLabel.Location = new System.Drawing.Point(20, 30);
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(120, 17);
        this.statusLabel.TabIndex = 8;
        this.statusLabel.Text = "准备设置时间...";
        //
        // timer
        //
        this.timer.Interval = 1000;
        this.timer.Tick += new System.EventHandler(this.Timer_Tick);
        //
        // titleLabel
        //
        this.titleLabel.AutoSize = true;
        this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.titleLabel.ForeColor = System.Drawing.Color.DarkBlue;
        this.titleLabel.Location = new System.Drawing.Point(120, 20);
        this.titleLabel.Name = "titleLabel";
        this.titleLabel.Size = new System.Drawing.Size(160, 24);
        this.titleLabel.TabIndex = 9;
        this.titleLabel.Text = "WinLockTimer";
        //
        // timeSettingGroupBox
        //
        this.timeSettingGroupBox.Controls.Add(this.hoursNumericUpDown);
        this.timeSettingGroupBox.Controls.Add(this.minutesNumericUpDown);
        this.timeSettingGroupBox.Controls.Add(this.hoursLabel);
        this.timeSettingGroupBox.Controls.Add(this.minutesLabel);
        this.timeSettingGroupBox.Location = new System.Drawing.Point(50, 130);
        this.timeSettingGroupBox.Name = "timeSettingGroupBox";
        this.timeSettingGroupBox.Size = new System.Drawing.Size(300, 90);
        this.timeSettingGroupBox.TabIndex = 10;
        this.timeSettingGroupBox.TabStop = false;
        this.timeSettingGroupBox.Text = "设置时间";
        //
        // controlGroupBox
        //
        this.controlGroupBox.Controls.Add(this.startButton);
        this.controlGroupBox.Controls.Add(this.pauseButton);
        this.controlGroupBox.Controls.Add(this.resetButton);
        this.controlGroupBox.Location = new System.Drawing.Point(50, 230);
        this.controlGroupBox.Name = "controlGroupBox";
        this.controlGroupBox.Size = new System.Drawing.Size(300, 80);
        this.controlGroupBox.TabIndex = 11;
        this.controlGroupBox.TabStop = false;
        this.controlGroupBox.Text = "控制";
        //
        // statusGroupBox
        //
        this.statusGroupBox.Controls.Add(this.statusLabel);
        this.statusGroupBox.Location = new System.Drawing.Point(50, 320);
        this.statusGroupBox.Name = "statusGroupBox";
        this.statusGroupBox.Size = new System.Drawing.Size(300, 70);
        this.statusGroupBox.TabIndex = 12;
        this.statusGroupBox.TabStop = false;
        this.statusGroupBox.Text = "状态";
        //
        // settingsGroupBox
        //
        this.settingsGroupBox.Controls.Add(this.clearSettingsButton);
        this.settingsGroupBox.Controls.Add(this.reminderTypeComboBox);
        this.settingsGroupBox.Controls.Add(this.reminderTypeLabel);
        this.settingsGroupBox.Controls.Add(this.passwordTextBox);
        this.settingsGroupBox.Controls.Add(this.passwordLabel);
        this.settingsGroupBox.Location = new System.Drawing.Point(50, 400);
        this.settingsGroupBox.Name = "settingsGroupBox";
        this.settingsGroupBox.Size = new System.Drawing.Size(300, 130);
        this.settingsGroupBox.TabIndex = 13;
        this.settingsGroupBox.TabStop = false;
        this.settingsGroupBox.Text = "设置";
        //
        // reminderTypeComboBox
        //
        this.reminderTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.reminderTypeComboBox.FormattingEnabled = true;
        this.reminderTypeComboBox.Items.AddRange(new object[] {
            "弹窗提醒",
            "语音提醒",
            "弹窗+语音提醒"});
        this.reminderTypeComboBox.Location = new System.Drawing.Point(150, 55);
        this.reminderTypeComboBox.Name = "reminderTypeComboBox";
        this.reminderTypeComboBox.Size = new System.Drawing.Size(130, 23);
        this.reminderTypeComboBox.TabIndex = 3;
        this.reminderTypeComboBox.SelectedIndex = 0;
        //
        // reminderTypeLabel
        //
        this.reminderTypeLabel.AutoSize = true;
        this.reminderTypeLabel.Location = new System.Drawing.Point(150, 35);
        this.reminderTypeLabel.Name = "reminderTypeLabel";
        this.reminderTypeLabel.Size = new System.Drawing.Size(59, 15);
        this.reminderTypeLabel.TabIndex = 2;
        this.reminderTypeLabel.Text = "提醒方式";
        //
        // passwordTextBox
        //
        this.passwordTextBox.Location = new System.Drawing.Point(20, 55);
        this.passwordTextBox.Name = "passwordTextBox";
        this.passwordTextBox.PasswordChar = '*';
        this.passwordTextBox.PlaceholderText = "输入家长密码";
        this.passwordTextBox.Size = new System.Drawing.Size(120, 23);
        this.passwordTextBox.TabIndex = 1;
        //
        // passwordLabel
        //
        this.passwordLabel.AutoSize = true;
        this.passwordLabel.Location = new System.Drawing.Point(20, 35);
        this.passwordLabel.Name = "passwordLabel";
        this.passwordLabel.Size = new System.Drawing.Size(59, 15);
        this.passwordLabel.TabIndex = 0;
        this.passwordLabel.Text = "家长密码";
        //
        // clearSettingsButton
        //
        this.clearSettingsButton.BackColor = System.Drawing.Color.LightGray;
        this.clearSettingsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.clearSettingsButton.Location = new System.Drawing.Point(20, 90);
        this.clearSettingsButton.Name = "clearSettingsButton";
        this.clearSettingsButton.Size = new System.Drawing.Size(120, 25);
        this.clearSettingsButton.TabIndex = 4;
        this.clearSettingsButton.Text = "清除设置";
        this.clearSettingsButton.UseVisualStyleBackColor = false;
        this.clearSettingsButton.Click += new System.EventHandler(this.ClearSettingsButton_Click);
        //
        // MainForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(400, 550);
        this.Controls.Add(this.settingsGroupBox);
        this.Controls.Add(this.statusGroupBox);
        this.Controls.Add(this.controlGroupBox);
        this.Controls.Add(this.timeSettingGroupBox);
        this.Controls.Add(this.titleLabel);
        this.Controls.Add(this.timeDisplayLabel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "WinLockTimer - 家长控制程序";
        ((System.ComponentModel.ISupportInitialize)(this.hoursNumericUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.minutesNumericUpDown)).EndInit();
        this.timeSettingGroupBox.ResumeLayout(false);
        this.timeSettingGroupBox.PerformLayout();
        this.controlGroupBox.ResumeLayout(false);
        this.statusGroupBox.ResumeLayout(false);
        this.statusGroupBox.PerformLayout();
        this.settingsGroupBox.ResumeLayout(false);
        this.settingsGroupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private Label timeDisplayLabel;
    private NumericUpDown hoursNumericUpDown;
    private NumericUpDown minutesNumericUpDown;
    private Label hoursLabel;
    private Label minutesLabel;
    private Button startButton;
    private Button pauseButton;
    private Button resetButton;
    private Label statusLabel;
    private System.Windows.Forms.Timer timer;
    private Label titleLabel;
    private GroupBox timeSettingGroupBox;
    private GroupBox controlGroupBox;
    private GroupBox statusGroupBox;
    private GroupBox settingsGroupBox;
    private Button clearSettingsButton;
    private TextBox passwordTextBox;
    private Label passwordLabel;
    private ComboBox reminderTypeComboBox;
    private Label reminderTypeLabel;
}
