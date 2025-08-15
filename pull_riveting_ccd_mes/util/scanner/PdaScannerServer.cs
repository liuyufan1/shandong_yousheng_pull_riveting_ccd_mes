using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using pull_riveting_ccd_mes.util.mes;

namespace pull_riveting_ccd_mes.util.scanner;

/// <summary>
/// 这个类处理pda扫描到的条码
/// </summary>

/// <summary>
/// 这个类处理 PDA 扫描到的条码
/// </summary>
public class PdaScannerServer
{
    private readonly HttpListener _listener;
    private bool _isRunning;

    /// <summary>
    /// 每次收到 HTTP PUT 请求时触发
    /// </summary>
    public event Func<PdaBarcodeEntity,ResEntity> OnBarcodeReceived;

    /// <summary>
    /// 初始化并监听指定 URL 前缀（必须以 / 结尾）
    /// 例如: http://localhost:8080/pda/
    /// </summary>
    public PdaScannerServer(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    public void Start()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine("PDA 扫描服务已启动，等待 PUT 请求...");
        _ = ListenLoop();
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
    }

    /// <summary>
    /// 循环监听 HTTP 请求
    /// </summary>
    private async Task ListenLoop()
    {
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
            catch (HttpListenerException)
            {
                // 监听被停止
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("监听异常: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// 处理单个 HTTP 请求
    /// </summary>
    private async Task HandleRequest(HttpListenerContext context)
    {
        // 全局添加 CORS 头
        context.Response.AddHeader("Access-Control-Allow-Origin", "*"); // 开发可以用 *
        context.Response.AddHeader("Access-Control-Allow-Methods", "PUT, OPTIONS");
        context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

        // OPTIONS 预检请求直接返回 200
        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.StatusCode = 200;
            context.Response.Close(); // 必须 Close，否则浏览器拿不到头
            return;
        }

        if (context.Request.HttpMethod == "PUT")
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                string body = await reader.ReadToEndAsync();

                var entity = JsonSerializer.Deserialize<PdaBarcodeEntity>(body);

                if (entity != null)
                {
                    ResEntity returnJson = OnBarcodeReceived.Invoke(entity);
                    await RespondJson(context, 200, new { code = returnJson.Code, message = returnJson.Message });
                    return;
                }

                await RespondJson(context, 400, new { code = 400, message = "请求体为空" });
            }
            catch (Exception ex)
            {
                await RespondJson(context, 500, new { code = 500, message = ex.Message });
            }
        }
        else
        {
            await RespondJson(context, 405, new { code = 405, message = "Method Not Allowed" });
        }
    }


    /// <summary>
    /// 返回 JSON 响应
    /// </summary>
    private async Task RespondJson(HttpListenerContext context, int statusCode, object obj)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        string json = JsonSerializer.Serialize(obj);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        context.Response.Close();
    }
}