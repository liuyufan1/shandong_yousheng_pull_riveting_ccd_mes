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
    /// <param name="isReplenishment">是否为补码，如果是补码则直接上传mes。</param>
    /// <returns></returns>
    public static ResEntity OnReceived(string barcode, string machineName, bool isReplenishment)
    {
        if (isReplenishment)
        {
            return IsReplenishment(barcode, machineName);
        }
        else
        {
            return IsNotReplenishment(barcode, machineName);
        }
    }
    
    private static ResEntity IsReplenishment(string barcode, string machineName)
    {
        string response = "";
        switch (machineName)
        {
            case "MS11抽芯拉铆左":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var upload = MesUtil.Upload(barcode, CorePullingRivetingManage.MS11Left.Processes,
                    CorePullingRivetingManage.MS11Left.Board, CorePullingRivetingManage.MS11Left.UserName, "", "22", "");
                response = upload.Result.Message;
                break;
            case "MS11抽芯拉铆右":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var task = MesUtil.Upload(barcode, CorePullingRivetingManage.MS11Right.Processes,
                    CorePullingRivetingManage.MS11Right.Board, CorePullingRivetingManage.MS11Right.UserName, "", "22", "");
                response = task.Result.Message;
                break;
            case "MX11抽芯拉铆左":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var upload1 = MesUtil.Upload(barcode, CorePullingRivetingManage.MX11Left.Processes,
                    CorePullingRivetingManage.MX11Left.Board, CorePullingRivetingManage.MX11Left.UserName, "", "补码", "");
                response = upload1.Result.Message;
                break;
            case "MX11抽芯拉铆右":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var task1 = MesUtil.Upload(barcode, CorePullingRivetingManage.MX11Right.Processes,
                    CorePullingRivetingManage.MX11Right.Board, CorePullingRivetingManage.MX11Right.UserName, "", "补码", "");
                response = task1.Result.Message;
                break;
            case "MS11CCD":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var upload2 = MesUtil.Upload(barcode, CCDManage.MS11CCD.Processes, CCDManage.MS11CCD.Board, CCDManage.MS11CCD.UserName, "", "补码", "");
                response = upload2.Result.Message;
                break;
            case "MX11CCD":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var task2 = MesUtil.Upload(barcode, CCDManage.MX11CCD.Processes, CCDManage.MX11CCD.Board, CCDManage.MX11CCD.UserName, "", "补码", "");
                response = task2.Result.Message;
                break;
            // 伺服拉铆1或2只需要扫一次
            case "伺服拉铆1":
            case "伺服拉铆2":
                LogUtil.AddLog("[伺服拉铆1]补码"  + ", 扫码成功: " + barcode);
                LogUtil.AddLog("[伺服拉铆2]补码"  + ", 扫码成功: " + barcode);
                var upload3 = MesUtil.Upload(barcode, ServoRivetingManage.ServoRiveting1.Processes,
                    ServoRivetingManage.ServoRiveting1.Board, ServoRivetingManage.ServoRiveting1.UserName, "", "18016,4832,3;18016,4791,3;18026,4569,3",
                    "");
                response += upload3.Result.Message;
                var task3 = MesUtil.Upload(barcode, ServoRivetingManage.ServoRiveting2.Processes, 
                    ServoRivetingManage.ServoRiveting2.Board, ServoRivetingManage.ServoRiveting2.UserName, "", "18016,4832,3;18016,4791,3;18026,4569,3",
                    "");
                response += task3.Result.Message;

                break;
            case "伺服拉铆3":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var upload4 = MesUtil.Upload(barcode, ServoRivetingManage.ServoRiveting3.Processes,
                    ServoRivetingManage.ServoRiveting3.Board, ServoRivetingManage.ServoRiveting3.UserName, "", "18016,4832,3;18016,4791,3;18026,4569,3",
                    "");
                response = upload4.Result.Message;
                break;
            case "伺服拉铆4":
                LogUtil.AddLog("[" + machineName + "]补码"  + ", 扫码成功: " + barcode);
                var task4 = MesUtil.Upload(barcode, ServoRivetingManage.ServoRiveting4.Processes,
                    ServoRivetingManage.ServoRiveting4.Board, ServoRivetingManage.ServoRiveting4.UserName, "", "18016,4832,3;18016,4791,3;18026,4569,3",
                    "");
                response = task4.Result.Message;
                break;
            
            default:
                LogUtil.AddLog("machineName 补码匹配失败：" + "[" + machineName + "]" );
                return ResEntity.Fail(401, "machineName 匹配失败：" + "[" + machineName + "]" );
                
        }
        return ResEntity.Success("扫码成功" + response);
    }
    
    private static ResEntity IsNotReplenishment(string barcode, string machineName)
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