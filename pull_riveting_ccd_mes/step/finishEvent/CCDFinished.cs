using HslCommunication.Profinet.Siemens;
using pull_riveting_ccd_mes.deviceManager.ccd;
using Serilog;

namespace pull_riveting_ccd_mes.step.finishEvent;

/// <summary>
/// CCD完成检测类
/// </summary>
public class CCDFinished
{
    private SiemensS7Net ms11CCD;
    private SiemensS7Net mx11CCD;

    private bool lastBoolMS11 = false;
    private bool lastBoolMX11 = false;

    // 上升沿触发事件，参数：CCD名称，Int值，DInt值
    public event Action<string, short, int> OnCCDTriggered;

    public CCDFinished()
    {
        // 初始化 PLC 对象
        ms11CCD = new SiemensS7Net(SiemensPLCS.S1200, "192.168.23.10");
        mx11CCD = new SiemensS7Net(SiemensPLCS.S1200, "192.168.23.15");
    }

    public void Start()
    {
        // 连接两台 PLC
        var resMS11 = ms11CCD.ConnectServer();
        var resMX11 = mx11CCD.ConnectServer();

        if (!resMS11.IsSuccess)
        {
            Log.Information("[MS11CCD] 连接失败: " + resMS11.Message);
            return;
        }

        if (!resMX11.IsSuccess)
        {
            Log.Information("[MX11CCD] 连接失败: " + resMX11.Message);
            return;
        }

        Log.Information("[MS11CCD] 和 [MX11CCD] 已连接");

        // 启动线程不停监听
        Thread tMS11 = new Thread(() => MonitorPLC(ms11CCD, ref lastBoolMS11, "MS11CCD"));
        Thread tMX11 = new Thread(() => MonitorPLC(mx11CCD, ref lastBoolMX11, "MX11CCD"));

        tMS11.IsBackground = true;
        tMX11.IsBackground = true;

        tMS11.Start();
        tMX11.Start();
    }

    private void MonitorPLC(SiemensS7Net plc, ref bool lastBool, string ccdName)
    {
        while (true)
        {
            try
            {
                // 读取 DB500.6 bool
                var boolRes = plc.ReadBool("DB500.6");
                if (!boolRes.IsSuccess)
                {
                    Log.Information($"[{ccdName}] bool读取失败: {boolRes.Message}");
                    Thread.Sleep(50);
                    continue;
                }

                bool currentBool = boolRes.Content;

                // 上升沿检测
                if (!lastBool && currentBool)
                {
                    try
                    {
                        Log.Information($"[{ccdName}] 上升沿触发");

                        // 统一读取 DB500.0 Int16 和 DB500.2 DInt32
                        var intRes = plc.ReadInt16("DB500.0");
                        short intValue = intRes.IsSuccess ? intRes.Content : (short)0;

                        var dIntRes = plc.ReadInt32("DB500.2");
                        int dIntValue = dIntRes.IsSuccess ? dIntRes.Content : 0;

                        Log.Information($"[{ccdName}] DB500.0 Int16: {intValue}, DB500.2 DInt32: {dIntValue}");

                        // 触发事件给外部使用
                        OnCCDTriggered?.Invoke(ccdName, intValue, dIntValue);

                        // CCD特有逻辑
                        if (ccdName == "MS11CCD")
                        {
                            Log.Information("[MS11CCD]收到ok信号");
                            CCDManage.MS11CCD.Data = new CCDData() { Result = intValue, DetailedInformation = dIntValue };
                            CCDManage.MS11CCD.SendToMes();
                        }
                        else if (ccdName == "MX11CCD")
                        {
                            Log.Information("[MX11CCD]收到ok信号");
                            CCDManage.MX11CCD.Data = new CCDData() { Result = intValue, DetailedInformation = dIntValue };
                            CCDManage.MX11CCD.SendToMes();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Information($"[{ccdName}] 触发事件异常: {ex.Message}");
                    }
                    
                }

                lastBool = currentBool;
                Thread.Sleep(50); // 每50ms读取一次
            }
            catch (Exception ex)
            {
                Log.Information($"[{ccdName}] 监控异常: {ex.Message}");
            }
        }
    }
}