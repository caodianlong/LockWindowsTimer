using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WinLockTimer;

static class Program
{
    /// <summary>
    /// 应用程序互斥体，用于防止多开
    /// </summary>
    private static Mutex? mutex;

    // Windows API 常量
    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;

    // Windows API 函数
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // 创建互斥体，确保只有一个实例运行
        const string mutexName = "WinLockTimer_SingleInstance_Mutex";
        bool createdNew;

        try
        {
            mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // 已有实例在运行，激活已存在的窗口
                ActivateExistingInstance();
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"启动程序时发生错误：{ex.Message}",
                "错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            // 释放互斥体
            mutex?.ReleaseMutex();
            mutex?.Dispose();
        }
    }

    /// <summary>
    /// 激活已存在的程序实例窗口
    /// </summary>
    private static void ActivateExistingInstance()
    {
        try
        {
            // 通过窗口标题查找已存在的窗口
            const string windowTitle = "WinLockTimer - 家长控制程序";
            IntPtr hWnd = FindWindow(null, windowTitle);

            if (hWnd != IntPtr.Zero)
            {
                // 如果窗口被最小化，先恢复它
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
                else
                {
                    // 如果窗口正常显示，确保它可见
                    ShowWindow(hWnd, SW_SHOW);
                }

                // 将窗口带到前台
                SetForegroundWindow(hWnd);
            }
            else
            {
                // 如果通过标题找不到，尝试查找WinForms的主窗口类
                const string className = "WindowsForms10.Window.8.app.0";
                hWnd = FindWindow(className, null);

                if (hWnd != IntPtr.Zero)
                {
                    if (IsIconic(hWnd))
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                    }
                    else
                    {
                        ShowWindow(hWnd, SW_SHOW);
                    }

                    SetForegroundWindow(hWnd);
                }
            }
        }
        catch (Exception ex)
        {
            // 如果激活失败，静默处理，不显示错误信息
            System.Diagnostics.Debug.WriteLine($"激活已存在窗口失败: {ex.Message}");
        }
    }
}
