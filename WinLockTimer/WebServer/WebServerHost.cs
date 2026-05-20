namespace WinLockTimer.WebServer;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EmbedIO;
using EmbedIO.Files;
using EmbedIO.WebApi;

/// <summary>
/// 嵌入式 HTTP Server 管理器
/// 在后台线程运行 EmbedIO Web Server
/// </summary>
public class WebServerHost : IDisposable
{
    private EmbedIO.WebServer? _server;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private readonly string _ip;
    private readonly int _port;
    private readonly bool _enableHttps;
    private readonly int _httpsPort;
    private readonly string _accessToken;

    public bool IsRunning => _server?.State == WebServerState.Listening;
    public string Url => _enableHttps ? $"https://{_ip}:{_httpsPort}" : $"http://{_ip}:{_port}";

    public WebServerHost(string ip, int port, bool enableHttps, int httpsPort, string accessToken)
    {
        _ip = string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        _port = port;
        _enableHttps = enableHttps;
        _httpsPort = httpsPort;
        _accessToken = accessToken;
    }

    /// <summary>
    /// 启动 Web Server
    /// </summary>
    public void Start()
    {
        if (_server != null) return;

        try
        {
            // 静态文件目录
            var wwwrootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

            // 确保 wwwroot 目录存在
            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
                Debug.WriteLine($"Web Server: wwwroot 目录不存在，已自动创建: {wwwrootPath}");
            }

            var options = new WebServerOptions()
                .WithMode(HttpListenerMode.EmbedIO);

            if (_enableHttps)
            {
                var certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinLockTimer.pfx");
                X509Certificate2 cert;
                if (File.Exists(certPath))
                {
                    cert = new X509Certificate2(certPath, "winlocktimer");
                }
                else
                {
                    using var rsa = RSA.Create(2048);
                    var req = new CertificateRequest($"CN={_ip}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    var rawCert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
                    var pfxBytes = rawCert.Export(X509ContentType.Pfx, "winlocktimer");
                    File.WriteAllBytes(certPath, pfxBytes);
                    cert = new X509Certificate2(pfxBytes, "winlocktimer", X509KeyStorageFlags.Exportable);
                }
                options.WithCertificate(cert);
            }

            // HttpListener 严格校验 Host 头。
            // 如果只绑定 127.0.0.1，使用 localhost 访问会被拒绝，所以需要同时绑定。
            if (_ip == "127.0.0.1" || _ip.ToLower() == "localhost")
            {
                options.WithUrlPrefix($"http://127.0.0.1:{_port}");
                options.WithUrlPrefix($"http://localhost:{_port}");
                if (_enableHttps)
                {
                    options.WithUrlPrefix($"https://127.0.0.1:{_httpsPort}");
                    options.WithUrlPrefix($"https://localhost:{_httpsPort}");
                }
            }
            else if (_ip == "0.0.0.0" || _ip == "*" || _ip == "+")
            {
                options.WithUrlPrefix($"http://+:{_port}");
                if (_enableHttps)
                {
                    options.WithUrlPrefix($"https://+:{_httpsPort}");
                }
            }
            else
            {
                options.WithUrlPrefix($"http://{_ip}:{_port}");
                if (_enableHttps)
                {
                    options.WithUrlPrefix($"https://{_ip}:{_httpsPort}");
                }
            }

            _server = new EmbedIO.WebServer(options)
                // 注册认证模块（全局中间件）
                .WithModule(new AuthModule("/", _accessToken))
                // 注册 REST API 控制器
                .WithWebApi("/api", m => m.WithController(() => new ApiController(_accessToken)))
                // 注册静态文件服务（放在最后，作为 fallback）
                .WithStaticFolder("/", wwwrootPath, true, m =>
                {
                    m.DefaultDocument = "index.html";
                    m.ContentCaching = false; // 开发阶段不缓存
                });

            _cts = new CancellationTokenSource();

            // 在后台线程启动 Server
            _serverTask = _server.RunAsync(_cts.Token);

            Debug.WriteLine($"Web Server 已启动: http://localhost:{_port}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Web Server 启动失败: {ex.Message}");
            _server?.Dispose();
            _server = null;
        }
    }

    /// <summary>
    /// 停止 Web Server
    /// </summary>
    public void Stop()
    {
        try
        {
            _cts?.Cancel();

            // 等待服务器停止（最多3秒）
            if (_serverTask != null)
            {
                _serverTask.Wait(TimeSpan.FromSeconds(3));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Web Server 停止异常: {ex.Message}");
        }
        finally
        {
            _server?.Dispose();
            _server = null;
            _cts?.Dispose();
            _cts = null;
            _serverTask = null;

            Debug.WriteLine("Web Server 已停止");
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
