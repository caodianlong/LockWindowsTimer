namespace WinLockTimer.WebServer;

using System;
using System.Threading.Tasks;
using EmbedIO;

/// <summary>
/// Access Token 认证中间件
/// 拦截 /api/ 路径下的请求，验证 Authorization: Bearer <token>
/// 静态文件请求不需要认证
/// </summary>
public class AuthModule : WebModuleBase
{
    private readonly string _accessToken;

    public AuthModule(string baseRoute, string accessToken)
        : base(baseRoute)
    {
        _accessToken = accessToken;
    }

    public override bool IsFinalHandler => false;

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        var path = context.RequestedPath;

        // 静态文件和认证验证端点不需要认证
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return; // 放行，继续下一个模块处理
        }

        // /api/auth/verify 端点不需要认证（用于验证 Token 本身）
        if (path.Equals("/api/auth/verify", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 检查 Authorization Header
        var authHeader = context.Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authHeader))
        {
            context.Response.StatusCode = 401;
            await context.SendStringAsync(
                "{\"error\":\"缺少认证信息\",\"code\":\"UNAUTHORIZED\"}",
                "application/json",
                System.Text.Encoding.UTF8);
            context.SetHandled();
            return;
        }

        // 解析 Bearer Token
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.SendStringAsync(
                "{\"error\":\"认证格式错误\",\"code\":\"INVALID_FORMAT\"}",
                "application/json",
                System.Text.Encoding.UTF8);
            context.SetHandled();
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // 验证 Token
        if (!string.Equals(token, _accessToken, StringComparison.Ordinal))
        {
            context.Response.StatusCode = 401;
            await context.SendStringAsync(
                "{\"error\":\"Token 无效\",\"code\":\"INVALID_TOKEN\"}",
                "application/json",
                System.Text.Encoding.UTF8);
            context.SetHandled();
            return;
        }

        // Token 验证通过，继续处理
    }
}
