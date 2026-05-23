# WinLockTimer - 家长控制电脑使用时间程序

## 项目概述

WinLockTimer 是一个Windows桌面应用程序，用于家长控制电脑使用时间。程序启动一个倒计时器，在倒计时结束前会弹出提醒，倒计时结束后自动锁屏。

## 功能需求

### 核心功能
1. **倒计时设置**
   - 用户可设置倒计时时长
   - 支持小时和分钟设置

2. **倒计时提醒**
   - 倒计时结束前10分钟弹出提醒
   - 倒计时结束前5分钟弹出提醒
   - 倒计时结束前4分钟弹出提醒
   - 倒计时结束前3分钟弹出提醒
   - 倒计时结束前2分钟弹出提醒
   - 倒计时结束前1分钟弹出提醒

3. **自动锁屏**
   - 倒计时结束后自动执行Windows锁屏命令
   - 使用Windows系统锁屏功能

### 用户界面需求
- 简洁直观的GUI界面
- 倒计时显示
- 设置倒计时时长
- 开始/暂停/重置功能
- 当前状态显示

## 技术选型

### 开发语言和框架
- **语言**: C#
- **GUI框架**: Windows Forms (WinForms)
- **目标平台**: Windows 10/11
- **.NET版本**: .NET 6.0 或更高版本

### 技术优势
- C# 是Windows桌面应用开发的首选语言
- WinForms 提供简单快速的GUI开发
- 良好的Windows系统集成能力
- 成熟的定时器和系统API支持

## 系统架构设计

### 主要组件

1. **主窗体 (MainForm)**
   - 倒计时显示
   - 时间设置控件
   - 控制按钮（开始、暂停、重置）
   - 状态显示

2. **倒计时管理器 (TimerManager)**
   - 管理倒计时逻辑
   - 处理提醒触发
   - 控制锁屏执行

3. **提醒服务 (NotificationService)**
   - 管理多个提醒时间点
   - 显示提醒对话框
   - 处理用户交互

4. **锁屏服务 (LockScreenService)**
   - 执行Windows锁屏命令
   - 系统集成

### 核心类设计

```csharp
// 主窗体类
public class MainForm : Form
{
    // UI控件
    private Label timeDisplay;
    private NumericUpDown hoursInput;
    private NumericUpDown minutesInput;
    private Button startButton;
    private Button pauseButton;
    private Button resetButton;
    private Label statusLabel;

    // 业务逻辑
    private TimerManager timerManager;
}

// 倒计时管理器
public class TimerManager
{
    private TimeSpan remainingTime;
    private TimeSpan totalTime;
    private System.Windows.Forms.Timer timer;
    private List<TimeSpan> reminderTimes;
    private bool isRunning;

    public event Action<TimeSpan> TimeUpdated;
    public event Action<string> StatusChanged;
    public event Action ReminderTriggered;
    public event Action TimeExpired;
}

// 提醒服务
public class NotificationService
{
    public void ShowReminder(string message);
    public void ShowFinalWarning();
}

// 锁屏服务
public class LockScreenService
{
    public void LockScreen();
}
```

## 实现细节

### 倒计时逻辑
- 使用System.Windows.Forms.Timer实现精确计时
- 每秒更新一次显示
- 支持暂停和继续功能

### 提醒机制
- 预定义提醒时间点：[10, 5, 4, 3, 2, 1] 分钟
- 每个提醒时间点只触发一次
- 提醒对话框显示剩余时间

### 锁屏实现
- 使用Windows系统命令：`rundll32.exe user32.dll,LockWorkStation`
- 确保跨Windows版本兼容性

## 用户界面设计

### 布局设计
```
┌─────────────────────────────────┐
│ WinLockTimer - 家长控制程序     │
├─────────────────────────────────┤
│ 剩余时间: 01:30:00              │
│                                 │
│ 设置时间: [1] 小时 [30] 分钟    │
│                                 │
│ [开始] [暂停] [重置]            │
│                                 │
│ 状态: 运行中...                 │
└─────────────────────────────────┘
```

### 控件说明
- **时间显示**: 大字体显示剩余时间
- **时间设置**: 数字输入框设置小时和分钟
- **控制按钮**: 开始、暂停、重置倒计时
- **状态显示**: 显示当前程序状态

## 开发计划

### 阶段1: 基础框架
- 创建项目结构和主窗体
- 实现基本的倒计时显示
- 实现时间设置功能

### 阶段2: 核心功能
- 实现倒计时逻辑
- 添加开始/暂停/重置功能
- 实现状态管理

### 阶段3: 提醒功能
- 实现提醒时间点检测
- 添加提醒对话框
- 处理提醒逻辑

### 阶段4: 锁屏功能
- 实现Windows锁屏
- 测试系统集成
- 错误处理

### 阶段5: 完善和测试
- 用户界面优化
- 异常处理
- 完整功能测试

## 部署和运行

### 系统要求
- Windows 10 或更高版本
- .NET 6.0 Runtime 或更高版本

### 构建说明
- 使用Visual Studio或.NET CLI构建
- 生成独立的可执行文件
- 支持直接运行，无需安装

## 扩展功能考虑

### 未来可能的增强
1. **声音提醒**: 添加声音提示
2. **自定义提醒时间**: 允许用户自定义提醒时间点
3. **密码保护**: 防止孩子关闭程序
4. **使用记录**: 记录每次使用情况
5. **多语言支持**: 支持中文和英文界面

## 注意事项

### 安全考虑
- 程序不应请求管理员权限
- 锁屏功能使用标准Windows API
- 不修改系统设置

### 用户体验
- 界面简洁直观
- 操作简单明了
- 状态清晰可见
- 错误提示友好

这个设计文档为WinLockTimer的开发提供了完整的指导，确保程序能够满足家长控制电脑使用时间的核心需求。