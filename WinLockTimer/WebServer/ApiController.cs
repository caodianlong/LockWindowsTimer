namespace WinLockTimer.WebServer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using WinLockTimer.Data;
using WinLockTimer.Models;
using WinLockTimer.Services;

/// <summary>
/// REST API 控制器
/// 提供倒计时状态查询、控制操作、账户和历史记录查询
/// </summary>
public class ApiController : WebApiController
{
    private readonly string _accessToken;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ApiController(string accessToken)
    {
        _accessToken = accessToken;
    }

    #region 认证

    /// <summary>
    /// 验证 Access Token 是否有效
    /// POST /api/auth/verify
    /// </summary>
    [Route(HttpVerbs.Post, "/auth/verify")]
    public async Task VerifyToken()
    {
        try
        {
            var body = await HttpContext.GetRequestBodyAsStringAsync();
            var request = JsonSerializer.Deserialize<TokenVerifyRequest>(body, _jsonOptions);

            if (request != null && string.Equals(request.Token, _accessToken, StringComparison.Ordinal))
            {
                await SendJsonResponse(new { success = true, message = "Token 验证成功" });
            }
            else
            {
                HttpContext.Response.StatusCode = 401;
                await SendJsonResponse(new { success = false, message = "Token 无效" });
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            await SendJsonResponse(new { success = false, message = $"请求格式错误: {ex.Message}" });
        }
    }

    #endregion

    #region 倒计时控制

    /// <summary>
    /// 获取当前倒计时状态
    /// GET /api/timer/status
    /// </summary>
    [Route(HttpVerbs.Get, "/timer/status")]
    public async Task GetTimerStatus()
    {
        var status = TimerService.Instance.GetStatus();
        await SendJsonResponse(status);
    }

    /// <summary>
    /// 启动倒计时
    /// POST /api/timer/start
    /// Body: { "hours": 1, "minutes": 30, "accountId": -1 }
    /// </summary>
    [Route(HttpVerbs.Post, "/timer/start")]
    public async Task StartTimer()
    {
        try
        {
            var body = await HttpContext.GetRequestBodyAsStringAsync();
            var request = JsonSerializer.Deserialize<TimerStartRequest>(body, _jsonOptions);

            if (request == null)
            {
                HttpContext.Response.StatusCode = 400;
                await SendJsonResponse(new { success = false, message = "请求格式错误" });
                return;
            }

            var result = TimerService.Instance.Start(request.Hours, request.Minutes, request.AccountId);

            if (result)
            {
                await SendJsonResponse(new { success = true, message = "倒计时已启动" });
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await SendJsonResponse(new { success = false, message = "无法启动倒计时（可能已在运行中或时间无效）" });
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            await SendJsonResponse(new { success = false, message = $"请求处理失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 暂停倒计时
    /// POST /api/timer/pause
    /// </summary>
    [Route(HttpVerbs.Post, "/timer/pause")]
    public async Task PauseTimer()
    {
        // 暂停需要验证家长密码（与桌面端行为一致）
        var passwordError = await VerifyParentPassword();
        if (passwordError != null)
        {
            HttpContext.Response.StatusCode = 403;
            await SendJsonResponse(new { success = false, message = passwordError });
            return;
        }

        var result = TimerService.Instance.Pause();
        if (result)
        {
            await SendJsonResponse(new { success = true, message = "倒计时已暂停" });
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
            await SendJsonResponse(new { success = false, message = "无法暂停（倒计时未运行或已暂停）" });
        }
    }

    /// <summary>
    /// 继续倒计时
    /// POST /api/timer/resume
    /// </summary>
    [Route(HttpVerbs.Post, "/timer/resume")]
    public async Task ResumeTimer()
    {
        // 继续操作需要验证家长密码
        var passwordError = await VerifyParentPassword();
        if (passwordError != null)
        {
            HttpContext.Response.StatusCode = 403;
            await SendJsonResponse(new { success = false, message = passwordError });
            return;
        }

        var result = TimerService.Instance.Resume();
        if (result)
        {
            await SendJsonResponse(new { success = true, message = "倒计时已继续" });
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
            await SendJsonResponse(new { success = false, message = "无法继续（倒计时未暂停）" });
        }
    }

    /// <summary>
    /// 重置倒计时
    /// POST /api/timer/reset
    /// </summary>
    [Route(HttpVerbs.Post, "/timer/reset")]
    public async Task ResetTimer()
    {
        // 重置操作需要验证家长密码
        var passwordError = await VerifyParentPassword();
        if (passwordError != null)
        {
            HttpContext.Response.StatusCode = 403;
            await SendJsonResponse(new { success = false, message = passwordError });
            return;
        }

        var result = TimerService.Instance.Reset();
        if (result)
        {
            await SendJsonResponse(new { success = true, message = "倒计时已重置" });
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
            await SendJsonResponse(new { success = false, message = "重置失败" });
        }
    }

    #endregion

    #region 账户和历史

    /// <summary>
    /// 获取所有账户
    /// GET /api/accounts
    /// </summary>
    [Route(HttpVerbs.Get, "/accounts")]
    public async Task GetAccounts()
    {
        try
        {
            if (!DatabaseManager.IsDatabaseAvailable())
            {
                await SendJsonResponse(new { accounts = Array.Empty<object>() });
                return;
            }

            var repo = new AccountRepository();
            var accounts = repo.GetAllAccounts();
            var result = accounts.Select(a => new
            {
                id = a.Id,
                username = a.Username,
                createdAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });

            await SendJsonResponse(new { accounts = result });
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await SendJsonResponse(new { success = false, message = $"查询账户失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取历史记录
    /// GET /api/history?accountId=1&startDate=2025-01-01&endDate=2025-12-31
    /// </summary>
    [Route(HttpVerbs.Get, "/history")]
    public async Task GetHistory()
    {
        try
        {
            if (!DatabaseManager.IsDatabaseAvailable())
            {
                await SendJsonResponse(new { records = Array.Empty<object>() });
                return;
            }

            // 解析查询参数
            int? accountId = null;
            DateTime? startDate = null;
            DateTime? endDate = null;

            var queryAccountId = HttpContext.GetRequestQueryData()["accountId"];
            if (!string.IsNullOrEmpty(queryAccountId) && int.TryParse(queryAccountId, out var aId) && aId > 0)
            {
                accountId = aId;
            }

            var queryStart = HttpContext.GetRequestQueryData()["startDate"];
            if (!string.IsNullOrEmpty(queryStart) && DateTime.TryParse(queryStart, out var sd))
            {
                startDate = sd.Date;
            }
            else
            {
                // 默认查最近7天
                startDate = DateTime.Today.AddDays(-7);
            }

            var queryEnd = HttpContext.GetRequestQueryData()["endDate"];
            if (!string.IsNullOrEmpty(queryEnd) && DateTime.TryParse(queryEnd, out var ed))
            {
                endDate = ed.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                endDate = DateTime.Now;
            }

            var recordRepo = new TimerRecordRepository();
            var accountRepo = new AccountRepository();
            var records = recordRepo.GetRecords(accountId, startDate, endDate);
            var accounts = accountRepo.GetAllAccounts();

            var result = records.Select(r =>
            {
                var acc = accounts.FirstOrDefault(a => a.Id == r.AccountId);
                return new
                {
                    id = r.Id,
                    accountId = r.AccountId,
                    username = acc?.Username ?? "(未知/默认)",
                    startTime = r.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = r.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    duration = TimeSpan.FromSeconds(r.DurationSeconds).ToString(@"hh\:mm\:ss")
                };
            });

            await SendJsonResponse(new { records = result });
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await SendJsonResponse(new { success = false, message = $"查询历史记录失败: {ex.Message}" });
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 验证家长密码
    /// 返回 null 表示验证通过，返回字符串表示错误信息
    /// </summary>
    private async Task<string?> VerifyParentPassword()
    {
        try
        {
            var settings = SettingsManager.LoadSettings();

            // 如果未设置家长密码，直接允许
            if (string.IsNullOrEmpty(settings.ParentPassword))
            {
                return null;
            }

            // 从请求体中获取密码
            var body = await HttpContext.GetRequestBodyAsStringAsync();
            PasswordRequest? request = null;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    request = System.Text.Json.JsonSerializer.Deserialize<PasswordRequest>(body, _jsonOptions);
                }
                catch { /* 解析失败时 request 保持 null */ }
            }

            if (request == null || string.IsNullOrEmpty(request.Password))
            {
                return "需要家长密码验证";
            }

            // BCrypt 验证
            if (settings.ParentPassword.StartsWith("$2") && settings.ParentPassword.Length >= 59)
            {
                if (BCrypt.Net.BCrypt.Verify(request.Password, settings.ParentPassword))
                {
                    return null; // 验证通过
                }
            }
            else
            {
                // 旧格式明文密码对比
                if (request.Password == settings.ParentPassword)
                {
                    return null;
                }
            }

            return "密码错误";
        }
        catch (Exception ex)
        {
            return $"密码验证失败: {ex.Message}";
        }
    }

    private async Task SendJsonResponse(object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await HttpContext.SendStringAsync(json, "application/json", System.Text.Encoding.UTF8);
    }

    #endregion
}

#region 请求 DTO

public class TokenVerifyRequest
{
    public string Token { get; set; } = string.Empty;
}

public class TimerStartRequest
{
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int AccountId { get; set; } = -1;
}

public class PasswordRequest
{
    public string Password { get; set; } = string.Empty;
}

#endregion
