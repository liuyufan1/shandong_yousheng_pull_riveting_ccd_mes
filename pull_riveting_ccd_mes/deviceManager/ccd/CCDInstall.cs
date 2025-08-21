using pull_riveting_ccd_mes.programUtil.log;
using pull_riveting_ccd_mes.util.mes;

namespace pull_riveting_ccd_mes.deviceManager.ccd;

public class CCDInstall : DeviceFather<CCDData>
{

    public CCDInstall(string name, string board, string userName, string process)
    {
        Name = name;
        Board = board;
        UserName = userName;
        Processes = process;
    }

    // public ResEntity SendToMes()
    // {
    //     for (int i = 0; i < 3; i++)
    //     {
    //         var result = MesUtil.Upload(NowBarcode, Processes, Board, UserName,"","", Data?.ToString()??"");
    //         var resultResult = result.Result;
    //         if (resultResult.Code == 200)
    //         {
    //             return resultResult;
    //         }
    //     }
    //
    //     LogUtil.ShowInMainPgae(Name + " mes连续上传失败3次。条码：" + NowBarcode);
    //     NowBarcode = "";
    //     
    //     return ResEntity.Fail(500, "上传失败3次");
    // }
    
}