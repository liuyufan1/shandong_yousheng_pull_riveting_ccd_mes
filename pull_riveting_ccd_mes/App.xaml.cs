using System.Configuration;
using System.Data;
using System.Windows;
using pull_riveting_ccd_mes.deviceManager.ccd;
using pull_riveting_ccd_mes.deviceManager.CorePullingRiveting;
using pull_riveting_ccd_mes.deviceManager.ServoRiveting;
using pull_riveting_ccd_mes.programUtil.log;
using pull_riveting_ccd_mes.step;
using pull_riveting_ccd_mes.step.finishEvent;
using pull_riveting_ccd_mes.util.scanner;

namespace pull_riveting_ccd_mes;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private PdaScannerServer? _pdaScannerServer;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        
        SerilogCustom.Init();
        try
        {
            
            // 全局异常捕获
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            base.OnStartup(e);
            // 启动 PDA 扫码服务
            _pdaScannerServer = new PdaScannerServer("http://+:8089/pdaScanner/");
            _pdaScannerServer.OnBarcodeReceived +=
                entity => PdaBarcodeReceived.OnReceived(entity.barcode, entity.machineName, entity.isReplenishment);
            _pdaScannerServer.Start();
            // 配置CCD管理类
            CCDManage.Init();
            // 监听CCD完成信号
            new CCDFinished().Start();
            // 配置伺服拉铆管理类
            ServoRivetingManage.Init();
            // 监听伺服拉铆完成信号
            ServoFinished.StartListen();
            // 配置抽芯拉铆管理类
            CorePullingRivetingManage.Init();
            // 监听抽芯拉铆完成信号
            new CorePullingRivetingFinished().Start();
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "启动应用程序失败");
            MessageBox.Show($"启动失败: {ex.Message}");
            Environment.Exit(1);
        }
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        _pdaScannerServer?.Stop();
        base.OnExit(e);
        SerilogCustom.Shutdown();
    }
    
    // UI 线程未捕获异常
    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogUtil.AddLog($"UI线程异常: {e.Exception}");
        Serilog.Log.Error(e.Exception, "UI线程未处理异常");
        e.Handled = true; // 设置为已处理，防止程序崩溃
    }

    // 非 UI 线程未捕获异常
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogUtil.AddLog($"非UI线程异常: {ex}");
            Serilog.Log.Error(ex, "非UI线程未处理异常");
        }
    }

    // 未观察的异步异常
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogUtil.AddLog($"未观察的Task异常: {e.Exception}");
        Serilog.Log.Error(e.Exception, "未观察的Task异常");
        e.SetObserved(); // 避免程序终止
    }
}