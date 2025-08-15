using HslCommunication.Profinet.Siemens;
using pull_riveting_ccd_mes.deviceManager.CorePullingRiveting;
using pull_riveting_ccd_mes.programUtil.log;
using Serilog;

namespace pull_riveting_ccd_mes.step.finishEvent;

public class CorePullingRivetingFinished
{
    private readonly SiemensS7Net _ms11Left;
    private readonly SiemensS7Net _ms11Right;
    private readonly SiemensS7Net _mx11Left;
    private readonly SiemensS7Net _mx11Right;

    private bool _lastBoolMS11Left = false;
    private bool _lastBoolMS11Right = false;
    private bool _lastBoolMX11Left = false;
    private bool _lastBoolMX11Right = false;

    // 上升沿触发事件，参数：设备名称，Int值
    public event Action<string, short> OnDeviceTriggered;
    public CorePullingRivetingFinished()
    {
        _ms11Left = new SiemensS7Net(SiemensPLCS.S1200, "192.168.23.65");
        _ms11Right = new SiemensS7Net(SiemensPLCS.S1200, "192.168.23.70");

        // MX11Left 和 MX11Right 共用一个 IP
        _mx11Left = new SiemensS7Net(SiemensPLCS.S1200, "192.168.23.125"); // 假设 IP
        _mx11Right = _mx11Left; // 共用同一个对象
    }

    public void Start()
    {
        var resMS11Left = _ms11Left.ConnectServer();
        var resMS11Right = _ms11Right.ConnectServer();
        var resMX11 = string.IsNullOrEmpty(_mx11Left.IpAddress) ? null : _mx11Left.ConnectServer();

        if (!resMS11Left.IsSuccess)
        {
            Log.Information("抽芯拉铆MS11Left 连接失败: " + resMS11Left.Message);
            return;
        }
        if (!resMS11Right.IsSuccess)
        {
            Log.Information("抽芯拉铆MS11Right 连接失败: " + resMS11Right.Message);
            return;
        }
        if (resMX11 != null && !resMX11.IsSuccess)
        {
            Log.Information("抽芯拉铆MX11 连接失败: " + resMX11.Message);
            return;
        }

        Log.Information("[抽芯拉铆] - 所有已配置的设备连接成功");

        Thread tMS11Left = new Thread(() => MonitorPLC(_ms11Left, ref _lastBoolMS11Left, "MS11Left", "DB500.2", "DB500.0"));
        Thread tMS11Right = new Thread(() => MonitorPLC(_ms11Right, ref _lastBoolMS11Right, "MS11Right", "DB500.2", "DB500.0"));

        // MX11Left/Right 共用一个 PLC，但不同地址
        Thread tMX11Left = new Thread(() => MonitorPLC(_mx11Left, ref _lastBoolMX11Left, "MX11Left", "DB500.4", "DB500.0"));
        Thread tMX11Right = new Thread(() => MonitorPLC(_mx11Right, ref _lastBoolMX11Right, "MX11Right", "DB500.5", "DB500.2"));

        tMS11Left.IsBackground = true;
        tMS11Right.IsBackground = true;
        tMX11Left.IsBackground = true;
        tMX11Right.IsBackground = true;

        tMS11Left.Start();
        tMS11Right.Start();
        tMX11Left.Start();
        tMX11Right.Start();
    }

    private void MonitorPLC(SiemensS7Net plc, ref bool lastBool, string deviceName, string boolAddress, string intAddress)
    {
        if (string.IsNullOrEmpty(plc.IpAddress))
        {
            Log.Information($"{deviceName} 未配置 IP，跳过监控");
            return;
        }

        while (true)
        {
            try
            {
                var boolRes = plc.ReadBool(boolAddress);
                if (!boolRes.IsSuccess)
                {
                    Log.Information($"{deviceName} bool读取失败: {boolRes.Message}");
                    Thread.Sleep(50);
                    continue;
                }

                bool currentBool = boolRes.Content;

                if (!lastBool && currentBool) // 上升沿
                {
                    try
                    {
                        Log.Information($"[抽芯拉铆]{deviceName} 上升沿触发");

                        var intRes = plc.ReadInt16(intAddress);
                        short number = intRes.IsSuccess ? intRes.Content : (short)0;

                        Log.Information($"[抽芯拉铆]{deviceName} {intAddress} Int16: {number}");
                        OnDeviceTriggered?.Invoke(deviceName, number);

                        // 共用原有的设备处理逻辑
                        if (deviceName == "MS11Left")
                        {
                            LogUtil.ShowInMainPgae($"[抽芯拉铆]MS11Left 完成信号收到。拉铆数{number}");
                            if (CorePullingRivetingManage.MS11Left.Data != null)
                                CorePullingRivetingManage.MS11Left.Data.Number = number;
                            CorePullingRivetingManage.MS11Left.SendToMes();
                        }
                        else if (deviceName == "MS11Right")
                        {
                            LogUtil.ShowInMainPgae($"[抽芯拉铆]MS11Right 完成信号收到。拉铆数{number}");
                            if (CorePullingRivetingManage.MS11Right.Data != null)
                                CorePullingRivetingManage.MS11Right.Data.Number = number;
                            CorePullingRivetingManage.MS11Right.SendToMes();
                        }
                        else if (deviceName == "MX11Left")
                        {
                            LogUtil.ShowInMainPgae($"[抽芯拉铆]MX11Left 完成信号收到。拉铆数{number}");
                            if (CorePullingRivetingManage.MX11Left.Data != null)
                                CorePullingRivetingManage.MX11Left.Data.Number = number;
                            CorePullingRivetingManage.MX11Left.SendToMes();
                        }
                        else if (deviceName == "MX11Right")
                        {
                            LogUtil.ShowInMainPgae($"[抽芯拉铆]MX11Right 完成信号收到。拉铆数{number}");
                            if (CorePullingRivetingManage.MX11Right.Data != null)
                                CorePullingRivetingManage.MX11Right.Data.Number = number;
                            CorePullingRivetingManage.MX11Right.SendToMes();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Information($"[抽芯拉铆]{deviceName} 触发事件异常: {ex.Message}");
                    }
                }

                lastBool = currentBool;
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Log.Information($"{deviceName} 监控异常: {ex.Message}");
            }
        }
    }
}




// public class CorePullingRivetingFinished
// {
//     private readonly SiemensS7Net _ms11Left;
//     private readonly SiemensS7Net _ms11Right;
//     private readonly SiemensS7Net _mx11Left;
//     private readonly SiemensS7Net _mx11Right;
//
//     private bool _lastBoolMS11Left = false;
//     private bool _lastBoolMS11Right = false;
//     private bool _lastBoolMX11Left = false;
//     private bool _lastBoolMX11Right = false;
//
//     // 上升沿触发事件，参数：设备名称，Int值
//     public event Action<string, short> OnDeviceTriggered;
//
//     public CorePullingRivetingFinished()
//     {
//         // 初始化 PLC 对象
//         _ms11Left = new SiemensS7Net(SiemensPLCS.S1200, "192.168.23.65");
//         _ms11Right = new SiemensS7Net(SiemensPLCS.S1200, "192.168.23.70");
//
//         // 另外两台设备 IP 待定
//         _mx11Left = new SiemensS7Net(SiemensPLCS.S1200, "");
//         _mx11Right = new SiemensS7Net(SiemensPLCS.S1200, "");
//     }
//
//     public void Start()
//     {
//         // 连接 4 台 PLC
//         var resMS11Left = _ms11Left.ConnectServer();
//         var resMS11Right = _ms11Right.ConnectServer();
//         var resMX11Left = string.IsNullOrEmpty(_mx11Left.IpAddress) ? null : _mx11Left.ConnectServer();
//         var resMX11Right = string.IsNullOrEmpty(_mx11Right.IpAddress) ? null : _mx11Right.ConnectServer();
//
//         if (!resMS11Left.IsSuccess)
//         {
//             Log.Information("抽芯拉铆MS11Left 连接失败: " + resMS11Left.Message);
//             return;
//         }
//
//         if (!resMS11Right.IsSuccess)
//         {
//             Log.Information("抽芯拉铆MS11Right 连接失败: " + resMS11Right.Message);
//             return;
//         }
//
//         // if (resMX11Left != null && !resMX11Left.IsSuccess)
//         // {
//         //     Log.Information("抽芯拉铆MX11Left 连接失败: " + resMX11Left.Message);
//         //     return;
//         // }
//         //
//         // if (resMX11Right != null && !resMX11Right.IsSuccess)
//         // {
//         //     Log.Information("抽芯拉铆MX11Right 连接失败: " + resMX11Right.Message);
//         //     return;
//         // }
//
//         Log.Information("[抽芯拉铆] - 所有已配置的设备连接成功");
//
//         // 启动线程
//         Thread tMS11Left = new Thread(() => MonitorPLC(_ms11Left, ref _lastBoolMS11Left, "MS11Left"));
//         Thread tMS11Right = new Thread(() => MonitorPLC(_ms11Right, ref _lastBoolMS11Right, "MS11Right"));
//         Thread tMX11Left = new Thread(() => MonitorPLC(_mx11Left, ref _lastBoolMX11Left, "MX11Left"));
//         Thread tMX11Right = new Thread(() => MonitorPLC(_mx11Right, ref _lastBoolMX11Right, "MX11Right"));
//
//         tMS11Left.IsBackground = true;
//         tMS11Right.IsBackground = true;
//         tMX11Left.IsBackground = true;
//         tMX11Right.IsBackground = true;
//
//         tMS11Left.Start();
//         tMS11Right.Start();
//         tMX11Left.Start();
//         tMX11Right.Start();
//     }
//
//     private void MonitorPLC(SiemensS7Net plc, ref bool lastBool, string deviceName)
//     {
//         if (string.IsNullOrEmpty(plc.IpAddress))
//         {
//             Log.Information($"{deviceName} 未配置 IP，跳过监控");
//             return;
//         }
//
//         while (true)
//         {
//             try
//             {
//                 // 读取 DB500.2 bool 作为结束信号
//                 var boolRes = plc.ReadBool("DB500.2");
//                 if (!boolRes.IsSuccess)
//                 {
//                     Log.Information($"{deviceName} bool读取失败: {boolRes.Message}");
//                     Thread.Sleep(50);
//                     continue;
//                 }
//
//                 bool currentBool = boolRes.Content;
//
//                 // 上升沿检测
//                 if (!lastBool && currentBool)
//                 {
//                     try
//                     {
//                         Log.Information($"[抽芯拉铆]{deviceName} 上升沿触发");
//
//                         // 读取 DB500.0 Int16 是拉铆数量
//                         var intRes = plc.ReadInt16("DB500.0");
//                         short number = intRes.IsSuccess ? intRes.Content : (short)0;
//
//                         Log.Information($"[抽芯拉铆]{deviceName} DB500.0 Int16: {number}");
//
//                         // 触发事件给外部使用
//                         OnDeviceTriggered?.Invoke(deviceName, number);
//
//                         // 设备特有逻辑
//                         if (deviceName == "MS11Left")
//                         {
//                             LogUtil.ShowInMainPgae("[抽芯拉铆]MS11Left 完成信号收到。拉铆数" + number);
//                             if(CorePullingRivetingManage.MS11Left.Data != null) CorePullingRivetingManage.MS11Left.Data.Number = number;
//                             CorePullingRivetingManage.MS11Left.SendToMes();
//                         }
//                         else if (deviceName == "MS11Right")
//                         {
//                             LogUtil.ShowInMainPgae("[抽芯拉铆]MS11Right 完成信号收到。拉铆数" + number);
//                             if(CorePullingRivetingManage.MS11Right.Data != null) CorePullingRivetingManage.MS11Right.Data.Number = number;
//                             CorePullingRivetingManage.MS11Right.SendToMes();
//                         }
//                         else if (deviceName == "MX11Left")
//                         {
//                             LogUtil.ShowInMainPgae("[抽芯拉铆]MX11Left 完成信号收到。拉铆数" + number);
//                             if(CorePullingRivetingManage.MX11Left.Data != null) CorePullingRivetingManage.MX11Left.Data.Number = number;
//                             CorePullingRivetingManage.MX11Left.SendToMes();
//                         }
//                         else if (deviceName == "MX11Right")
//                         {
//                             LogUtil.ShowInMainPgae("[抽芯拉铆]MX11Right 完成信号收到。拉铆数" + number);
//                             if(CorePullingRivetingManage.MX11Right.Data != null) CorePullingRivetingManage.MX11Right.Data.Number = number;
//                             CorePullingRivetingManage.MX11Right.SendToMes();
//                         }
//                         else
//                         {
//                             LogUtil.ShowInMainPgae("[抽芯拉铆]未知的deviceName: " + deviceName);
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Information($"[抽芯拉铆]{deviceName} 触发事件异常: {ex.Message}");
//                     }
//                 }
//
//                 lastBool = currentBool;
//                 Thread.Sleep(50); // 每 50ms 读取一次
//             }
//             catch (Exception ex)
//             {
//                 Log.Information($"{deviceName} 监控异常: {ex.Message}");
//             }
//         }
//     }
// }
