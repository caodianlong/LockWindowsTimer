namespace WinLockTimer.Forms;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinLockTimer.Services;

/// <summary>
/// 进程名列表编辑窗体：允许家长自定义倒计时结束时要关闭的进程
/// </summary>
public class ProcessListEditForm : Form
{
    private TextBox processListTextBox;
    private Button resetDefaultButton;
    private Button okButton;
    private Button cancelButton;
    private Label hintLabel;

    /// <summary>
    /// 编辑后的进程名列表
    /// </summary>
    public List<string> ProcessNames { get; private set; } = new();

    public ProcessListEditForm(IEnumerable<string> currentNames)
    {
        InitializeUI();
        // 每行一个进程名
        processListTextBox.Text = string.Join(Environment.NewLine, currentNames);
    }

    private void InitializeUI()
    {
        this.Text = "编辑要关闭的程序列表";
        this.Size = new Size(400, 480);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        hintLabel = new Label
        {
            Text = "每行输入一个进程名（不需要 .exe 后缀）：\n例如：chrome、msedge、steam",
            Location = new Point(15, 15),
            Size = new Size(350, 40),
            AutoSize = false
        };

        processListTextBox = new TextBox
        {
            Location = new Point(15, 60),
            Size = new Size(350, 310),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10F),
            AcceptsReturn = true,
            WordWrap = false
        };

        resetDefaultButton = new Button
        {
            Text = "恢复默认列表",
            Location = new Point(15, 380),
            Size = new Size(110, 30),
            BackColor = Color.LightGray
        };
        resetDefaultButton.Click += (s, e) =>
        {
            processListTextBox.Text = string.Join(Environment.NewLine, ProcessKillerService.DefaultProcessNames);
        };

        okButton = new Button
        {
            Text = "确定",
            Location = new Point(170, 380),
            Size = new Size(90, 30),
            BackColor = Color.LightGreen,
            DialogResult = DialogResult.OK
        };
        okButton.Click += (s, e) =>
        {
            ProcessNames = processListTextBox.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        };

        cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(270, 380),
            Size = new Size(90, 30),
            BackColor = Color.LightCoral,
            DialogResult = DialogResult.Cancel
        };

        this.Controls.AddRange(new Control[] { hintLabel, processListTextBox, resetDefaultButton, okButton, cancelButton });
        this.AcceptButton = okButton;
        this.CancelButton = cancelButton;
    }
}
