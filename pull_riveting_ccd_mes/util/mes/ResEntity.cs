using System.Text.Json;

namespace pull_riveting_ccd_mes.util.mes;

public class ResEntity
{
    public int Code;
    public string Message;
    public object Data;

    public static ResEntity Success()
    {
        var resEntity = new ResEntity();
        resEntity.Code = 200;
        resEntity.Message = "成功";
        resEntity.Data = null;
        return resEntity;
    }

    public static ResEntity Success(string message)
    {
        var resEntity = new ResEntity();
        resEntity.Code = 200;
        resEntity.Message = message;
        resEntity.Data = null;
        return resEntity;
    }

    public static ResEntity Fail(int code, string errMsg)
    {
        var resEntity = new ResEntity();
        resEntity.Code = code;
        resEntity.Message = errMsg;
        resEntity.Data = null;
        return resEntity;
    }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
