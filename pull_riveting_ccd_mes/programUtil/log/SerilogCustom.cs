using Serilog;
using Serilog.Events;

namespace pull_riveting_ccd_mes.programUtil.log;

public class SerilogCustom
{
    public static void Init()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // 最低日志等级
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // 不让系统日志太多
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(
                path: "Logs\\log-.txt",
                rollingInterval: RollingInterval.Day, // 按天分日志文件
                retainedFileCountLimit: 30, // 最多保留 30 天
                fileSizeLimitBytes: 10_000_000, // 单文件大小限制（10MB）
                rollOnFileSizeLimit: true, // 超过大小自动换文件
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        Log.Information("Serilog 日志系统启动完成");
    }

    public static void Shutdown()
    {
        Log.Information("Serilog 日志系统即将关闭");
        Log.CloseAndFlush();
    }
    
}