namespace WinLockTimer.Forms;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinLockTimer.Data;
using WinLockTimer.Models;

public class AccountSelectionForm : Form
{
    private FlowLayoutPanel flowPanel;
    private AccountRepository _repository;
    public int SelectedAccountId { get; private set; } = -1;

    public AccountSelectionForm()
    {
        _repository = new AccountRepository();
        InitializeComponent();
        LoadAccounts();
    }

    private void InitializeComponent()
    {
        this.Text = "选择账户";
        this.Size = new Size(600, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20)
        };
        
        this.Controls.Add(flowPanel);
    }

    private void LoadAccounts()
    {
        var accounts = _repository.GetAllAccounts();
        if (accounts.Count == 0)
        {
            // Should not happen if logic in MainForm is correct, but handle anyway
            var lbl = new Label { Text = "没有可用账户", AutoSize = true, Font = new Font(this.Font.FontFamily, 14) };
            flowPanel.Controls.Add(lbl);
            return;
        }

        foreach (var acc in accounts)
        {
            var card = CreateAccountCard(acc);
            flowPanel.Controls.Add(card);
        }
    }

    private Control CreateAccountCard(Account account)
    {
        var panel = new Panel
        {
            Size = new Size(150, 200),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(10),
            Cursor = Cursors.Hand,
            BackColor = Color.White
        };
        
        // Avatar
        var pic = new PictureBox
        {
            Size = new Size(100, 100),
            Location = new Point(25, 20),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = null
        };
        
        if (!string.IsNullOrEmpty(account.AvatarPath) && File.Exists(account.AvatarPath))
        {
            try
            {
                using (var stream = new FileStream(account.AvatarPath, FileMode.Open, FileAccess.Read))
                {
                    pic.Image = Image.FromStream(stream);
                }
            }
            catch { }
        }
        
        if (pic.Image == null)
        {
             // Default placeholder or just a box
             pic.BackColor = Color.LightGray;
        }

        // Username
        var lbl = new Label
        {
            Text = account.Username,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Location = new Point(5, 130),
            Size = new Size(140, 30),
            Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
        };

        // Click handlers
        EventHandler clickHandler = (s, e) => SelectAccount(account);
        panel.Click += clickHandler;
        pic.Click += clickHandler;
        lbl.Click += clickHandler;

        panel.Controls.Add(pic);
        panel.Controls.Add(lbl);

        return panel;
    }

    private void SelectAccount(Account account)
    {
        if (!string.IsNullOrEmpty(account.PasswordHash))
        {
            // Prompt for password
            using (var pwdForm = new PasswordPromptForm(account.Username))
            {
                if (pwdForm.ShowDialog() == DialogResult.OK)
                {
                    if (BCrypt.Net.BCrypt.Verify(pwdForm.Password, account.PasswordHash))
                    {
                        ConfirmSelection(account.Id);
                    }
                    else
                    {
                        MessageBox.Show("密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        else
        {
            ConfirmSelection(account.Id);
        }
    }

    private void ConfirmSelection(int id)
    {
        SelectedAccountId = id;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    // Inner class for simple password prompt
    private class PasswordPromptForm : Form
    {
        public string Password { get; private set; } = "";
        private TextBox txtPwd;

        public PasswordPromptForm(string username)
        {
            this.Text = "输入密码";
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lbl = new Label { Text = $"请输入 {username} 的密码:", Location = new Point(20, 20), AutoSize = true };
            txtPwd = new TextBox { Location = new Point(20, 50), Width = 240, PasswordChar = '*' };
            var btnOk = new Button { Text = "确定", Location = new Point(180, 80), DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "取消", Location = new Point(80, 80), DialogResult = DialogResult.Cancel };

            btnOk.Click += (s, e) => { Password = txtPwd.Text; };
            
            this.Controls.Add(lbl);
            this.Controls.Add(txtPwd);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
        }
    }
}
