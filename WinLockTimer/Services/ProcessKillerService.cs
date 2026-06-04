namespace WinLockTimer.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// 进程关闭服务：倒计时结束时强制关闭指定的游戏和浏览器进程
/// </summary>
public static class ProcessKillerService
{
    /// <summary>
    /// 默认要关闭的进程名列表（不含 .exe 后缀）
    /// </summary>
    public static readonly string[] DefaultProcessNames = new[]
    {
        // 浏览器
        "chrome",
        "msedge",
        "firefox",
        "opera",
        "brave",
        // 游戏平台
        "steam",
        "steamwebhelper",
        "EpicGamesLauncher",
        "WeGame",
        "wegame_launcher",
    };

    /// <summary>
    /// 强制关闭指定进程名列表中的所有进程
    /// </summary>
    /// <param name="processNames">要关闭的进程名列表（不含 .exe）</param>
    /// <returns>成功关闭的进程信息</returns>
    public static List<string> KillProcesses(IEnumerable<string> processNames)
    {
        var killedList = new List<string>();

        foreach (var name in processNames)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;

            var trimmedName = name.Trim();
            // 如果用户输入了 .exe 后缀，去掉它
            if (trimmedName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                trimmedName = trimmedName[..^4];
            }

            try
            {
                var processes = Process.GetProcessesByName(trimmedName);
                foreach (var proc in processes)
                {
                    try
                    {
                        // 直接强制关闭，包括整个进程树
                        proc.Kill(entireProcessTree: true);
                        killedList.Add($"{trimmedName} (PID: {proc.Id})");
                    }
                    catch (Exception ex)
                    {
                        // 进程可能已经退出或无权限，记录但不中断
                        Debug.WriteLine($"关闭进程 {trimmedName} (PID: {proc.Id}) 失败: {ex.Message}");
                    }
                    finally
                    {
                        proc.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"查找进程 {trimmedName} 失败: {ex.Message}");
            }
        }

        return killedList;
    }

    /// <summary>
    /// 使用默认列表关闭进程
    /// </summary>
    /// <returns>成功关闭的进程信息</returns>
    public static List<string> KillDefaultProcesses()
    {
        return KillProcesses(DefaultProcessNames);
    }

    /// <summary>
    /// 将进程名列表序列化为逗号分隔的字符串（用于保存设置）
    /// </summary>
    public static string SerializeProcessNames(IEnumerable<string> names)
    {
        return string.Join(",", names.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()));
    }

    /// <summary>
    /// 从逗号分隔的字符串反序列化进程名列表
    /// </summary>
    public static List<string> DeserializeProcessNames(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return new List<string>(DefaultProcessNames);
        }

        return serialized
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
    }
}
