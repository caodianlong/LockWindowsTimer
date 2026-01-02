namespace WinLockTimer.Forms;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinLockTimer.Data;
using WinLockTimer.Models;

public class HistoryForm : Form
{
    private DataGridView gridHistory;
    private DateTimePicker dtpStart;
    private DateTimePicker dtpEnd;
    private ComboBox cmbAccounts;
    private Button btnQuery;
    
    private TimerRecordRepository _recordRepo;
    private AccountRepository _accountRepo;
    private List<Account> _accounts;

    public HistoryForm()
    {
        _recordRepo = new TimerRecordRepository();
        _accountRepo = new AccountRepository();
        InitializeComponent();
        LoadAccounts();
        // Default query
        PerformQuery();
    }

    private void InitializeComponent()
    {
        this.Text = "历史记录";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterParent;

        var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
        
        var lblStart = new Label { Text = "开始:", AutoSize = true, Location = new Point(10, 20) };
        dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-7), Location = new Point(50, 18), Width = 100 };
        
        var lblEnd = new Label { Text = "结束:", AutoSize = true, Location = new Point(160, 20) };
        dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Location = new Point(200, 18), Width = 100 };
        
        var lblAcc = new Label { Text = "账户:", AutoSize = true, Location = new Point(320, 20) };
        cmbAccounts = new ComboBox { Location = new Point(360, 18), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        
        btnQuery = new Button { Text = "查询", Location = new Point(500, 16), Width = 80 };
        btnQuery.Click += (s, e) => PerformQuery();

        panelTop.Controls.Add(lblStart);
        panelTop.Controls.Add(dtpStart);
        panelTop.Controls.Add(lblEnd);
        panelTop.Controls.Add(dtpEnd);
        panelTop.Controls.Add(lblAcc);
        panelTop.Controls.Add(cmbAccounts);
        panelTop.Controls.Add(btnQuery);

        gridHistory = new DataGridView 
        { 
            Dock = DockStyle.Fill, 
            AllowUserToAddRows = false, 
            AllowUserToDeleteRows = false, 
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        this.Controls.Add(gridHistory);
        this.Controls.Add(panelTop);
    }

    private void LoadAccounts()
    {
        _accounts = _accountRepo.GetAllAccounts();
        cmbAccounts.Items.Clear();
        cmbAccounts.Items.Add("所有账户");
        foreach (var acc in _accounts)
        {
            cmbAccounts.Items.Add(acc); // We might need to override ToString or wrap it
        }
        cmbAccounts.DisplayMember = "Username";
        cmbAccounts.SelectedIndex = 0;
    }

    private void PerformQuery()
    {
        int? selectedAccountId = null;
        if (cmbAccounts.SelectedIndex > 0) // 0 is All
        {
            var acc = cmbAccounts.SelectedItem as Account;
            if (acc != null) selectedAccountId = acc.Id;
        }

        // Adjust dates to cover full days
        DateTime start = dtpStart.Value.Date;
        DateTime end = dtpEnd.Value.Date.AddDays(1).AddTicks(-1);

        var records = _recordRepo.GetRecords(selectedAccountId, start, end);
        
        // Transform for display
        var displayList = records.Select(r => new 
        {
            Username = GetUsername(r.AccountId),
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Duration = TimeSpan.FromSeconds(r.DurationSeconds).ToString(@"hh\:mm\:ss")
        }).ToList();

        gridHistory.DataSource = displayList;
        
        // Rename columns
        if (gridHistory.Columns["Username"] != null) gridHistory.Columns["Username"].HeaderText = "用户名";
        if (gridHistory.Columns["StartTime"] != null) gridHistory.Columns["StartTime"].HeaderText = "开始时间";
        if (gridHistory.Columns["EndTime"] != null) gridHistory.Columns["EndTime"].HeaderText = "结束时间";
        if (gridHistory.Columns["Duration"] != null) gridHistory.Columns["Duration"].HeaderText = "时长";
    }

    private string GetUsername(int accountId)
    {
        var acc = _accounts.FirstOrDefault(a => a.Id == accountId);
        return acc != null ? acc.Username : "(未知/默认)";
    }
}
