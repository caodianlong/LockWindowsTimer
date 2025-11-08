namespace WinLockTimer;

using System;
using System.Windows.Forms;

// 密码修改窗体
public class ChangePasswordForm : Form
{
    private TextBox newPasswordTextBox;
    private TextBox confirmPasswordTextBox;
    private Button okButton;
    private Button cancelButton;
    private Label promptLabel;
    private Label newPasswordLabel;
    private Label confirmPasswordLabel;

    public string NewPassword => newPasswordTextBox.Text;

    public ChangePasswordForm()
    {
        InitializeComponent();
        this.Text = "修改家长密码";
    }

    private void InitializeComponent()
    {
        this.newPasswordTextBox = new TextBox();
        this.confirmPasswordTextBox = new TextBox();
        this.okButton = new Button();
        this.cancelButton = new Button();
        this.promptLabel = new Label();
        this.newPasswordLabel = new Label();
        this.confirmPasswordLabel = new Label();

        this.SuspendLayout();

        // promptLabel
        this.promptLabel.AutoSize = true;
        this.promptLabel.Location = new System.Drawing.Point(20, 20);
        this.promptLabel.Name = "promptLabel";
        this.promptLabel.Size = new System.Drawing.Size(200, 15);
        this.promptLabel.TabIndex = 0;
        this.promptLabel.Text = "请输入新的家长密码：";

        // newPasswordLabel
        this.newPasswordLabel.AutoSize = true;
        this.newPasswordLabel.Location = new System.Drawing.Point(20, 50);
        this.newPasswordLabel.Name = "newPasswordLabel";
        this.newPasswordLabel.Size = new System.Drawing.Size(59, 15);
        this.newPasswordLabel.TabIndex = 1;
        this.newPasswordLabel.Text = "新密码：";

        // newPasswordTextBox
        this.newPasswordTextBox.Location = new System.Drawing.Point(90, 47);
        this.newPasswordTextBox.Name = "newPasswordTextBox";
        this.newPasswordTextBox.PasswordChar = '*';
        this.newPasswordTextBox.Size = new System.Drawing.Size(150, 23);
        this.newPasswordTextBox.TabIndex = 2;

        // confirmPasswordLabel
        this.confirmPasswordLabel.AutoSize = true;
        this.confirmPasswordLabel.Location = new System.Drawing.Point(20, 80);
        this.confirmPasswordLabel.Name = "confirmPasswordLabel";
        this.confirmPasswordLabel.Size = new System.Drawing.Size(59, 15);
        this.confirmPasswordLabel.TabIndex = 3;
        this.confirmPasswordLabel.Text = "确认密码：";

        // confirmPasswordTextBox
        this.confirmPasswordTextBox.Location = new System.Drawing.Point(90, 77);
        this.confirmPasswordTextBox.Name = "confirmPasswordTextBox";
        this.confirmPasswordTextBox.PasswordChar = '*';
        this.confirmPasswordTextBox.Size = new System.Drawing.Size(150, 23);
        this.confirmPasswordTextBox.TabIndex = 4;

        // okButton
        this.okButton.Location = new System.Drawing.Point(20, 110);
        this.okButton.Name = "okButton";
        this.okButton.Size = new System.Drawing.Size(75, 25);
        this.okButton.TabIndex = 5;
        this.okButton.Text = "确定";
        this.okButton.UseVisualStyleBackColor = true;
        this.okButton.Click += OkButton_Click;

        // cancelButton
        this.cancelButton.Location = new System.Drawing.Point(105, 110);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Size = new System.Drawing.Size(75, 25);
        this.cancelButton.TabIndex = 6;
        this.cancelButton.Text = "取消";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        // ChangePasswordForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(260, 150);
        this.Controls.Add(this.cancelButton);
        this.Controls.Add(this.okButton);
        this.Controls.Add(this.confirmPasswordTextBox);
        this.Controls.Add(this.confirmPasswordLabel);
        this.Controls.Add(this.newPasswordTextBox);
        this.Controls.Add(this.newPasswordLabel);
        this.Controls.Add(this.promptLabel);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "修改密码";

        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        string newPassword = newPasswordTextBox.Text;
        string confirmPassword = confirmPasswordTextBox.Text;

        // 验证密码
        if (string.IsNullOrEmpty(newPassword))
        {
            // 如果新密码为空，表示用户要清除密码
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        }

        // 检查密码长度
        if (newPassword.Length < 4)
        {
            MessageBox.Show("密码长度不能少于4个字符！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            newPasswordTextBox.Focus();
            return;
        }

        // 检查两次输入的密码是否一致
        if (newPassword != confirmPassword)
        {
            MessageBox.Show("两次输入的密码不一致！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            confirmPasswordTextBox.Focus();
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}