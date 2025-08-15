using Serilog;

namespace pull_riveting_ccd_mes.programUtil.log;

public class LogUtil
{
    // 持有主窗口的日志控件引用（你需要先把它传进来）
    public static Action<string> ShowInMainPgae = s => { };

    /// <summary>
    /// 往日志控件追加一条日志（线程安全）
    /// </summary>
    public static void AddLog(string message)
    {
        if (ShowInMainPgae == null)
        {
            throw new InvalidOperationException("请先绑定 AppendLogAction");
        }

        Log.Information(message);
        // 调用主线程追加日志
        ShowInMainPgae.Invoke($"{DateTime.Now:HH:mm:ss} {message}");
    }
}