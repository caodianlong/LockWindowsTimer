namespace WinLockTimer;

using System;
using System.IO;
using System.Text.Json;

/// <summary>
/// 设置管理类，处理程序设置的保存和加载
/// </summary>
public static class SettingsManager
{
    private static readonly string SettingsFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "WinLockTimer.settings");

    /// <summary>
    /// 程序设置
    /// </summary>
    public class AppSettings
    {
        public string ParentPassword { get; set; } = string.Empty;
        public int ReminderType { get; set; } = 0; // 0: 弹窗, 1: 语音, 2: 两者
        public bool RememberSettings { get; set; } = true;
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    /// <param name="settings">设置对象</param>
    public static void SaveSettings(AppSettings settings)
    {
        try
        {
            // 如果密码不为空，则加密保存
            if (!string.IsNullOrEmpty(settings.ParentPassword))
            {
                string encryptedPassword = AesEncryption.Encrypt(settings.ParentPassword);

                // 创建临时设置对象，保存加密后的密码
                var tempSettings = new AppSettings
                {
                    ParentPassword = encryptedPassword,
                    ReminderType = settings.ReminderType,
                    RememberSettings = settings.RememberSettings
                };

                string json = JsonSerializer.Serialize(tempSettings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsFilePath, json);
            }
            else
            {
                // 如果没有密码，直接保存设置
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsFilePath, json);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"保存设置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 加载设置
    /// </summary>
    /// <returns>设置对象</returns>
    public static AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);

            if (settings == null)
            {
                return new AppSettings();
            }

            // 如果密码不为空，则尝试解密
            if (!string.IsNullOrEmpty(settings.ParentPassword))
            {
                try
                {
                    string decryptedPassword = AesEncryption.Decrypt(settings.ParentPassword);
                    settings.ParentPassword = decryptedPassword;
                }
                catch
                {
                    // 如果解密失败，可能是旧格式或损坏的数据，清空密码
                    settings.ParentPassword = string.Empty;
                }
            }

            return settings;
        }
        catch (Exception ex)
        {
            throw new Exception($"加载设置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除设置文件
    /// </summary>
    public static void DeleteSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                File.Delete(SettingsFilePath);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"删除设置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查设置文件是否存在
    /// </summary>
    /// <returns>是否存在</returns>
    public static bool SettingsExist()
    {
        return File.Exists(SettingsFilePath);
    }
}