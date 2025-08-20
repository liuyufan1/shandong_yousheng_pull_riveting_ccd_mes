namespace pull_riveting_ccd_mes.deviceManager.ccd;

public class CCDData
{
    // 视觉拍照检测总结果。 检测结果1为NG,2为OK
    public int Result { get; set; } 
    
    // 1为NG,2为OK,此数据包含多个面结果信息整合
    // 例如“22222”表示为5个面结果均OK,“22122”表示为面3结果为NG，其余面结果为OK
    public int DetailedInformation { get; set; }

    public override string ToString()
    {
        string res = Result == 1 ? "NG" : "OK";
        return res + "," + DetailedInformation;
    }
}