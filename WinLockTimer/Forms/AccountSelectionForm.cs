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
        ConfirmSelection(account.Id);
    }

    private void ConfirmSelection(int id)
    {
        SelectedAccountId = id;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
