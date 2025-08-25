using HslCommunication.Profinet.Siemens;
using pull_riveting_ccd_mes.deviceManager.ccd;
using Serilog;

namespace pull_riveting_ccd_mes.step.finishEvent;

/// <summary>
/// CCD完成检测 + 扫码完成检测类
/// </summary>
public class CCDFinished
{
    private SiemensS7Net ms11CCD;
    private SiemensS7Net mx11CCD;

    private bool lastBoolMS11 = false;
    private bool lastBoolMX11 = false;

    private bool lastScanMS11 = false;
    private bool lastScanMX11 = false;

    // CCD上升沿触发事件：CCD名称，Int值，DInt值
    public event Action<string, short, int> OnCCDTriggered;

    // 扫码上升沿触发事件：设备名称，条码值
    public event Action<string, string> OnBarcodeTriggered;

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
        Thread tMS11 = new Thread(() => MonitorPLC(ms11CCD, ref lastBoolMS11, ref lastScanMS11, "MS11CCD"));
        Thread tMX11 = new Thread(() => MonitorPLC(mx11CCD, ref lastBoolMX11, ref lastScanMX11, "MX11CCD"));

        tMS11.IsBackground = true;
        tMX11.IsBackground = true;

        tMS11.Start();
        tMX11.Start();
    }

    private void MonitorPLC(SiemensS7Net plc, ref bool lastBool, ref bool lastScan, string deviceName)
    {
        while (true)
        {
            try
            {
                // ================= CCD完成检测 =================
                var boolRes = plc.ReadBool("DB500.6");
                if (boolRes.IsSuccess)
                {
                    bool currentBool = boolRes.Content;

                    // CCD 上升沿
                    if (!lastBool && currentBool)
                    {
                        try
                        {
                            Log.Information($"[{deviceName}] CCD上升沿触发");

                            var intRes = plc.ReadInt16("DB500.0");
                            short intValue = intRes.IsSuccess ? intRes.Content : (short)0;

                            var dIntRes = plc.ReadInt32("DB500.2");
                            int dIntValue = dIntRes.IsSuccess ? dIntRes.Content : 0;

                            Log.Information($"[{deviceName}] DB500.0 Int16: {intValue}, DB500.2 DInt32: {dIntValue}");

                            OnCCDTriggered?.Invoke(deviceName, intValue, dIntValue);

                            if (deviceName == "MS11CCD")
                            {
                                CCDManage.MS11CCD.Data = new CCDData() { Result = intValue, DetailedInformation = dIntValue };
                                CCDManage.MS11CCD.SendToMes();
                            }
                            else if (deviceName == "MX11CCD")
                            {
                                CCDManage.MX11CCD.Data = new CCDData() { Result = intValue, DetailedInformation = dIntValue };
                                CCDManage.MX11CCD.SendToMes();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Information($"[{deviceName}] CCD触发异常: {ex.Message}");
                        }
                    }

                    lastBool = currentBool;
                }

                // ================= 扫码完成检测 =================
                var scanRes = plc.ReadBool("DB4.28.0");
                if (scanRes.IsSuccess)
                {
                    bool currentScan = scanRes.Content;

                    // 扫码上升沿
                    if (!lastScan && currentScan)
                    {
                        try
                        {
                            Log.Information($"[{deviceName}] 扫码信号上升沿触发");

                            // 读取条码字符串，假设条码在DB4.30.0开始，长度比如 50
                            var barcodeRes = plc.ReadString("DB4.30", 50);
                            string barcode = barcodeRes.IsSuccess ? barcodeRes.Content : string.Empty;

                            Log.Information($"[{deviceName}] 条码值: {barcode}");

                            OnBarcodeTriggered?.Invoke(deviceName, barcode);
                            
                            if (deviceName == "MS11CCD")
                            {
                                CCDManage.MS11CCD.NowBarcode = barcode;
                            }
                            else if (deviceName == "MX11CCD")
                            {
                                CCDManage.MX11CCD.NowBarcode = barcode;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Information($"[{deviceName}] 扫码触发异常: {ex.Message}");
                        }
                    }

                    lastScan = currentScan;
                }

                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Log.Information($"[{deviceName}] 监控异常: {ex.Message}");
            }
        }
    }
}
