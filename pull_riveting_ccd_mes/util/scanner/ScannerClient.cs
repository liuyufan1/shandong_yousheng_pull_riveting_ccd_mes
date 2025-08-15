using System.Net.Sockets;
using System.Text;

namespace pull_riveting_ccd_mes.util.scanner;

public class ScannerClient
{
    
    
    private TcpClient _client;
    private NetworkStream _stream;
    
    private string _ip;
    private int _port;

    /// <summary>
    /// 创建 TCP 连接（只执行一次）
    /// </summary>
    public ScannerClient(string ip, int port)
    {
        _ip = ip;
        _port = port;
        _client = new TcpClient();
        _client.Connect(_ip, _port);
        _stream = _client.GetStream();
    }

    /// <summary>
    /// 发送 "start" 并等待扫码枪返回数据
    /// 如果 IsSuccess 为 true，则返回扫码结果
    /// 如果 IsSuccess 为 false，则返回错误信息
    /// </summary>
    public async Task<(bool IsSuccess, string message)> GetScanner(int retryCount = 3, int retryDelayMs = 100)
    {
        if (_stream == null)
            throw new InvalidOperationException("请先调用 Start 建立连接");

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                // 1. 发送 start 命令
                byte[] startCmd = Encoding.ASCII.GetBytes("start\r\n");
                await _stream.WriteAsync(startCmd, 0, startCmd.Length);
                await _stream.FlushAsync();

                // 2. 等待扫码枪返回数据
                byte[] buffer = new byte[1024];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string result = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    
                    if (result == "noRead")
                        return (false, "扫码失败，请检查摄像头位置");
                    
                    return (true, result);
                }
                
                return (false, "读取到空数据");
                
            }
            catch (Exception ex)
            {
                // 最后一轮重试仍失败
                if (attempt == retryCount)
                {
                    return (false, $"尝试 {retryCount} 次后失败：{ex.Message}");
                }

                // 尝试重连
                try
                {
                    _stream.Close();
                    _client.Close();
                    _client = new TcpClient();
                    _client.Connect(_ip, _port);
                    _stream = _client.GetStream();
                }
                catch (Exception connEx)
                {
                    return (false, $"重连失败：{connEx.Message}");
                }

                // 等待后重试
                await Task.Delay(retryDelayMs);
            }
        }

        return (false, "未知错误");
    }


    /// <summary>
    /// 释放连接
    /// </summary>
    public void Dispose()
    {
        _stream?.Close();
        _client?.Close();
    }
}