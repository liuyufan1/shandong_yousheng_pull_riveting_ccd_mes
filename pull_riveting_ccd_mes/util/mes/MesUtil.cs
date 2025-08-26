using System.Net.Http;
using System.Text;
using System.Text.Json;
using Serilog;

namespace pull_riveting_ccd_mes.util.mes;

public class MesUtil
{
    
    private static readonly HttpClient _httpClient = new ();

    public static async Task<ResEntity> Upload(string barcode, string process,  string board, 
                                                string username, string heatNumber, string spec, 
                                                string alloy, string status = "A")
    {
        try
        {
            string url = "https://sh.unisonal.com:431/api/Common/AddEmploymentReportByCode";

            var payload = new
            {
                WBarCodeStr = "",
                BarCodeStr = barcode,
                Processes = process, // 工序名： ccd 抽芯拉铆 伺服拉铆
                Status = status,
                Board = board, // 机台编号
                Num = 1,
                UserName = username, // 机台名
                BatchNumber = "",
                HeatNumber = heatNumber,
                Spec = spec,
                Alloy = alloy,
                datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = JsonSerializer.Serialize(payload);
            Log.Information("MES请求json：" + json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                string msg = $"MES接口请求失败: HTTP {response.StatusCode}";
                Log.Error(msg);
                return ResEntity.Fail(500, msg);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            Log.Information($"MES返回: {responseString}");

            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("code", out var codeElement))
            {
                int code = codeElement.GetInt32();

                if (code == 0)
                {
                    return ResEntity.Success();
                }

                // 用枚举对应的消息替代接口返回的 message 字段
                string msg = GetMesResponseMessage(code);
                return ResEntity.Fail(500, msg);
            }

            string errorMsg = "MES返回数据没有 code 字段";
            Log.Warning(errorMsg);
            return ResEntity.Fail(500, errorMsg);
        }
        catch (Exception ex)
        {
            string errorMsg = $"MES接口调用异常: {ex.Message}";
            Log.Error(ex, errorMsg);
            return ResEntity.Fail(500, errorMsg);
        }
    }

    public static string GetMesResponseMessage(int code)
    {
        return code switch
        {
            0 => "成功",
            1 => "必传字段验证失败",
            2 => "条码不存在",
            3 => "已上传过条码",
            4 => "条码错误（解析失败，未找到型号或分公司代码）",
            5 => "已报工",
            6 => "条码为不良品",
            7 => "工序不存在",
            8 => "机台不存在",
            21 => "不良品",
            22 => "漏序",
            23 => "液冷板未关联",
            24 => "静置时间判断",
            404 => "请求数据为空",
            500 => "系统错误",
            _ => $"未知错误码: {code}"
        };
    }
}