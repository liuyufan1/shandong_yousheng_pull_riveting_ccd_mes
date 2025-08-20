using pull_riveting_ccd_mes.deviceManager.ccd;
using pull_riveting_ccd_mes.deviceManager.CorePullingRiveting;
using pull_riveting_ccd_mes.deviceManager.ServoRiveting;
using pull_riveting_ccd_mes.programUtil.log;
using pull_riveting_ccd_mes.util.mes;
using Serilog;

namespace pull_riveting_ccd_mes.step;

/// <summary>
/// pda扫码类
/// </summary>
public class PdaBarcodeReceived
{
    /// <summary>
    /// 接收到pda扫码信号后触发
    /// </summary>
    /// <param name="barcode">条码</param>
    /// <param name="machineName">扫码设备</param>
    /// <returns></returns>
    public static ResEntity OnReceived(string barcode, string machineName)
    {
        switch (machineName)
        {
            case "MS11抽芯拉铆左":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                CorePullingRivetingManage.MS11Left.NowBarcode = barcode;
                break;
            case "MS11抽芯拉铆右":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                CorePullingRivetingManage.MS11Right.NowBarcode = barcode;
                break;
            case "MX11抽芯拉铆左":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                CorePullingRivetingManage.MX11Left.NowBarcode = barcode;
                break;
            case "MX11抽芯拉铆右":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                CorePullingRivetingManage.MX11Right.NowBarcode = barcode;
                break;
            case "MS11CCD":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                CCDManage.MS11CCD.NowBarcode = barcode;
                break;
            case "MX11CCD":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                CCDManage.MX11CCD.NowBarcode = barcode;
                break;
            // case "伺服拉铆1":
            //     LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
            //     ServoRivetingManage.ServoRiveting1.NowBarcode = barcode;
            //     break;
            // case "伺服拉铆2":
            //     LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
            //     ServoRivetingManage.ServoRiveting2.NowBarcode = barcode;
            //     break;
            // 伺服拉铆1或2只需要扫一次
            case "伺服拉铆1":
            case "伺服拉铆2":
                LogUtil.AddLog("[伺服拉铆1]"  + ", 扫码成功: " + barcode);
                ServoRivetingManage.ServoRiveting1.NowBarcode = barcode;
                LogUtil.AddLog("[伺服拉铆2]"  + ", 扫码成功: " + barcode);
                ServoRivetingManage.ServoRiveting2.NowBarcode = barcode;
                break;
            case "伺服拉铆3":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                ServoRivetingManage.ServoRiveting3.NowBarcode = barcode;
                break;
            case "伺服拉铆4":
                LogUtil.AddLog("[" + machineName + "]"  + ", 扫码成功: " + barcode);
                ServoRivetingManage.ServoRiveting4.NowBarcode = barcode;
                break;
            
            default:
                LogUtil.AddLog("machineName 匹配失败：" + "[" + machineName + "]" );
                return ResEntity.Fail(401, "machineName 匹配失败：" + "[" + machineName + "]" );
                
        }
        return ResEntity.Success("扫码成功" + barcode);
    }
    
    
}