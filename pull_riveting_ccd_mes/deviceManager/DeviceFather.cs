using pull_riveting_ccd_mes.programUtil.log;
using pull_riveting_ccd_mes.util.mes;

namespace pull_riveting_ccd_mes.deviceManager;

public abstract class DeviceFather<T> where T : new()
{
    // 设备名称
    public string Name { get; set; }
    // 当前条码
    public string NowBarcode { get; set; }
    // Mes需要的机台编号
    public string Board { get; set; }
    // Mes需要的机台名
    public string UserName { get; set; }
    // Mes需要的工序名
    public string Processes { get; set; }
    
    public T? Data { get; set; }
    
    
    public ResEntity SendToMes()
    {
        ResEntity resultResult = new ResEntity();
        for (int i = 0; i < 3; i++)
        {
            var result = MesUtil.Upload(NowBarcode, Processes, Board, UserName, "", "", Data?.ToString()??"");
            resultResult = result.Result;
            if (resultResult.Code == 200)
            {
                LogUtil.ShowInMainPgae(Name + " mes上传成功。条码：" + NowBarcode);
                NowBarcode = "";
                Data = new T(); // 重置 Data
                return resultResult;
            }
        }

        LogUtil.ShowInMainPgae(Name + " mes连续上传失败3次。条码：" + NowBarcode + " " + resultResult.Message);
        NowBarcode = "";
        Data = new T(); // 重置 Data

        return ResEntity.Fail(500, "上传失败3次");
    }
}