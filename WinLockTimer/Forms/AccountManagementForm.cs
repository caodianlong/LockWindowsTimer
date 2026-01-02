namespace WinLockTimer.Forms;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinLockTimer.Data;
using WinLockTimer.Models;

public class AccountManagementForm : Form
{
    private ListBox lstAccounts;
    private Button btnAdd;
    private GroupBox grpDetails;
    private TextBox txtUsername;
    private PictureBox picAvatar;
    private Button btnSelectAvatar;
    private TextBox txtPassword;
    private Button btnSave;
    private Button btnDelete;
    private Label lblStatus; // To show validation errors

    private AccountRepository _repository;
    private List<Account> _accounts;
    private Account _currentAccount;
    private bool _isEditing = false; // true if editing existing, false if adding new (but usually we just select from list or click add to clear selection)

    public AccountManagementForm()
    {
        _repository = new AccountRepository();
        InitializeComponent();
        LoadAccounts();
    }

    private void InitializeComponent()
    {
        this.Text = "账户管理";
        this.Size = new Size(600, 450);
        this.MinimizeBox = false;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            IsSplitterFixed = true,
            SplitterDistance = 200
        };

        // Left Panel
        lstAccounts = new ListBox { Dock = DockStyle.Fill };
        lstAccounts.SelectedIndexChanged += LstAccounts_SelectedIndexChanged;
        
        var panelLeftBottom = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        btnAdd = new Button { Text = "新建账户", Dock = DockStyle.Fill };
        btnAdd.Click += BtnAdd_Click;
        panelLeftBottom.Controls.Add(btnAdd);

        splitContainer.Panel1.Controls.Add(lstAccounts);
        splitContainer.Panel1.Controls.Add(panelLeftBottom);

        // Right Panel
        grpDetails = new GroupBox { Text = "账户详情", Dock = DockStyle.Fill, Padding = new Padding(10) };
        
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Username
        layout.Controls.Add(new Label { Text = "用户名:", Anchor = AnchorStyles.Left }, 0, 0);
        txtUsername = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
        layout.Controls.Add(txtUsername, 1, 0);

        // Avatar
        layout.Controls.Add(new Label { Text = "头像:", Anchor = AnchorStyles.Top | AnchorStyles.Left }, 0, 1);
        var avatarPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown };
        picAvatar = new PictureBox { Size = new Size(100, 100), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        btnSelectAvatar = new Button { Text = "选择图片...", Width = 100 };
        btnSelectAvatar.Click += BtnSelectAvatar_Click;
        avatarPanel.Controls.Add(picAvatar);
        avatarPanel.Controls.Add(btnSelectAvatar);
        layout.Controls.Add(avatarPanel, 1, 1);

        // Password
        layout.Controls.Add(new Label { Text = "密码:", Anchor = AnchorStyles.Left }, 0, 2);
        txtPassword = new TextBox { PasswordChar = '*', Anchor = AnchorStyles.Left | AnchorStyles.Right, PlaceholderText = "留空则不修改" };
        layout.Controls.Add(txtPassword, 1, 2);

        // Actions
        var actionPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Top, Height = 40 };
        btnDelete = new Button { Text = "删除", ForeColor = Color.Red, Enabled = false };
        btnDelete.Click += BtnDelete_Click;
        btnSave = new Button { Text = "保存" };
        btnSave.Click += BtnSave_Click;
        actionPanel.Controls.Add(btnSave);
        actionPanel.Controls.Add(btnDelete);
        
        layout.Controls.Add(actionPanel, 1, 3);
        
        lblStatus = new Label { ForeColor = Color.Red, AutoSize = true };
        layout.Controls.Add(lblStatus, 1, 4);

        grpDetails.Controls.Add(layout);
        splitContainer.Panel2.Controls.Add(grpDetails);

        this.Controls.Add(splitContainer);
    }

    private void LoadAccounts()
    {
        _accounts = _repository.GetAllAccounts();
        lstAccounts.Items.Clear();
        foreach (var acc in _accounts)
        {
            lstAccounts.Items.Add(acc.Username);
        }
        ClearDetails();
    }

    private void LstAccounts_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstAccounts.SelectedIndex >= 0)
        {
            _currentAccount = _accounts[lstAccounts.SelectedIndex];
            _isEditing = true;
            
            txtUsername.Text = _currentAccount.Username;
            LoadAvatarImage(_currentAccount.AvatarPath);
            txtPassword.Text = ""; // Don't show hash
            txtPassword.PlaceholderText = "留空保持原密码";
            
            btnDelete.Enabled = true;
            grpDetails.Text = $"编辑账户: {_currentAccount.Username}";
        }
    }

    private void BtnAdd_Click(object sender, EventArgs e)
    {
        lstAccounts.SelectedIndex = -1;
        ClearDetails();
    }

    private void ClearDetails()
    {
        _currentAccount = null;
        _isEditing = false;
        txtUsername.Text = "";
        picAvatar.Image = null;
        txtPassword.Text = "";
        txtPassword.PlaceholderText = "设置新密码";
        btnDelete.Enabled = false;
        grpDetails.Text = "新建账户";
        lblStatus.Text = "";
    }

    private void LoadAvatarImage(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            picAvatar.Image = null;
            return;
        }
        try
        {
            // Use stream to avoid valid file locks? Actually Image.FromFile locks file.
            // Better creating a copy in memory if we just display, but for now simple FromFile.
            // Actually, if we use just FromFile, we can't overwrite it later if needed.
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                picAvatar.Image = Image.FromStream(stream);
            }
            picAvatar.Tag = path;
        }
        catch
        {
            picAvatar.Image = null;
        }
    }

    private void BtnSelectAvatar_Click(object sender, EventArgs e)
    {
        using (var ofd = new OpenFileDialog())
        {
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Load to view
                try
                {
                     using (var stream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        picAvatar.Image = Image.FromStream(stream);
                    }
                    picAvatar.Tag = ofd.FileName; // Store temp path
                }
                catch (Exception ex)
                {
                    MessageBox.Show("无法加载图片: " + ex.Message);
                }
            }
        }
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            lblStatus.Text = "用户名不能为空";
            return;
        }

        string newAvatarPath = "";
        
        // Handle Avatar Save
        if (picAvatar.Image != null && picAvatar.Tag is string tempPath)
        {
             // Determine if it is a new file or existing
             // If tempPath is not in our AppData/Avatars folder, copy it.
             string appAvatarDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Avatars");
             if (!Directory.Exists(appAvatarDir)) Directory.CreateDirectory(appAvatarDir);
             
             // Only copy if source is different
             if (!tempPath.StartsWith(appAvatarDir))
             {
                 string ext = Path.GetExtension(tempPath);
                 string destFileName = $"{Guid.NewGuid()}{ext}";
                 string destPath = Path.Combine(appAvatarDir, destFileName);
                 try
                 {
                     File.Copy(tempPath, destPath, true);
                     newAvatarPath = destPath;
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show("保存头像失败: " + ex.Message);
                     return;
                 }
             }
             else
             {
                 newAvatarPath = tempPath;
             }
        }
        else if (_isEditing && _currentAccount != null)
        {
            // Keep old if not changed
            newAvatarPath = _currentAccount.AvatarPath;
            // Unless image was cleared? (Image == null). Logic above covers Image != null.
            if (picAvatar.Image == null) newAvatarPath = "";
        }

        string passwordHash = "";
        if (!string.IsNullOrEmpty(txtPassword.Text))
        {
            passwordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text);
        }
        else if (_isEditing && _currentAccount != null)
        {
            passwordHash = _currentAccount.PasswordHash;
        }

        if (_isEditing && _currentAccount != null)
        {
            _currentAccount.Username = username;
            _currentAccount.AvatarPath = newAvatarPath;
            _currentAccount.PasswordHash = passwordHash;
            _repository.UpdateAccount(_currentAccount);
        }
        else
        {
            var newAccount = new Account
            {
                Username = username,
                AvatarPath = newAvatarPath,
                PasswordHash = passwordHash
            };
            _repository.AddAccount(newAccount);
        }

        LoadAccounts();
        lblStatus.Text = "保存成功";
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (_currentAccount == null) return;
        
        if (MessageBox.Show($"确定要删除账户 '{_currentAccount.Username}' 吗？\n相关的所有历史记录也将被删除！", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _repository.DeleteAccount(_currentAccount.Id);
            LoadAccounts();
        }
    }
}
