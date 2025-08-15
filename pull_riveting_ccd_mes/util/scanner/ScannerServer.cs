using System.Net;
using System.Net.Sockets;
using System.Text;

namespace pull_riveting_ccd_mes.util.scanner;

/// <summary>
/// 扫码枪扫到的条码
/// </summary>
public class ScannerServer
{
    
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private Task _listenTask;

    public event Action<string> OnBarcodeReceived;
    /// <summary>
    /// 启动服务器
    /// </summary>
    public void Start(string ip, int port)
    {
        if (_listener != null)
            throw new InvalidOperationException("服务器已经启动");

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        _listener.Start();

        _listenTask = Task.Run(() => ListenForClients(_cts.Token));

        Console.WriteLine($"[ScannerServer] 启动成功，监听 {ip}:{port}");
    }

    private async Task ListenForClients(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();
                string remote = client.Client.RemoteEndPoint?.ToString();
                Console.WriteLine($"[ScannerServer] 客户端已连接: {remote}");

                _ = Task.Run(() => HandleClient(client, remote, token), token);
            }
        }
        catch (ObjectDisposedException)
        {
            // 监听器已关闭
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScannerServer] 监听异常: {ex.Message}");
        }
    }

    private async Task HandleClient(TcpClient client, string remote, CancellationToken token)
    {
        using (client)
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead <= 0)
                        break; // 客户端断开

                    string barcode = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"[ScannerServer] 收到条码: {barcode}");

                    OnBarcodeReceived?.Invoke(barcode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScannerServer] 客户端 {remote} 异常: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"[ScannerServer] 客户端已断开: {remote}");
            }
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _listener = null;
    }

    public void Dispose()
    {
        Stop();
    }
}