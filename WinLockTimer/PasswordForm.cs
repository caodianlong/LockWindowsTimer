namespace WinLockTimer;

using System;
using System.Windows.Forms;

// 密码输入窗体
public class PasswordForm : Form
{
    private TextBox passwordTextBox;
    private Button okButton;
    private Button cancelButton;
    private Label promptLabel;

    public string Password => passwordTextBox.Text;

    public PasswordForm(string action)
    {
        InitializeComponent();
        promptLabel.Text = $"请输入家长密码以{action}：";
        this.Text = $"验证密码 - {action}";
    }

    private void InitializeComponent()
    {
        this.passwordTextBox = new TextBox();
        this.okButton = new Button();
        this.cancelButton = new Button();
        this.promptLabel = new Label();

        this.SuspendLayout();

        // promptLabel
        this.promptLabel.AutoSize = true;
        this.promptLabel.Location = new System.Drawing.Point(20, 20);
        this.promptLabel.Name = "promptLabel";
        this.promptLabel.Size = new System.Drawing.Size(200, 15);
        this.promptLabel.TabIndex = 0;

        // passwordTextBox
        this.passwordTextBox.Location = new System.Drawing.Point(20, 50);
        this.passwordTextBox.Name = "passwordTextBox";
        this.passwordTextBox.PasswordChar = '*';
        this.passwordTextBox.Size = new System.Drawing.Size(200, 23);
        this.passwordTextBox.TabIndex = 1;

        // okButton
        this.okButton.Location = new System.Drawing.Point(20, 90);
        this.okButton.Name = "okButton";
        this.okButton.Size = new System.Drawing.Size(75, 25);
        this.okButton.TabIndex = 2;
        this.okButton.Text = "确定";
        this.okButton.UseVisualStyleBackColor = true;
        this.okButton.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

        // cancelButton
        this.cancelButton.Location = new System.Drawing.Point(105, 90);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Size = new System.Drawing.Size(75, 25);
        this.cancelButton.TabIndex = 3;
        this.cancelButton.Text = "取消";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        // PasswordForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(240, 130);
        this.Controls.Add(this.cancelButton);
        this.Controls.Add(this.okButton);
        this.Controls.Add(this.passwordTextBox);
        this.Controls.Add(this.promptLabel);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "验证密码";

        this.ResumeLayout(false);
        this.PerformLayout();
    }
}