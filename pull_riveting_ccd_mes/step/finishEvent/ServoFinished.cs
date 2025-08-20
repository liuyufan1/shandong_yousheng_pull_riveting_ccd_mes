using System.Text;
using HslCommunication;
using HslCommunication.MQTT;
using Newtonsoft.Json.Linq;
using pull_riveting_ccd_mes.deviceManager.ServoRiveting;
using pull_riveting_ccd_mes.programUtil.log;
using Serilog;

namespace pull_riveting_ccd_mes.step.finishEvent;

public class ServoFinished
{
    public static MqttClient MqttClient;
    
    public static MqttSyncClient MqttSyncClient;

    public static void StartListen()
    {
        try
        {
            MqttClient = new MqttClient(new MqttConnectionOptions()
            {

                ClientId = "ABC",
                IpAddress = "127.0.0.1",
                Port = 521,
                Credentials = new MqttCredential("admin", "123456"), // 设置了用户名和密码

            });
            MqttSyncClient = new MqttSyncClient(new MqttConnectionOptions()
            {

                IpAddress = "127.0.0.1",
                Port = 521,
                Credentials = new MqttCredential("admin", "123456"), // 设置了用户名和密码
                ConnectTimeout = 2000
            });
            MqttSyncClient.SetPersistentConnection();
            MqttClient.ConnectServer();

            // mqtt事件处理
            MqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;
            Log.Information("订阅了伺服拉铆枪");

            // 伺服拉铆枪完成
            MqttClient.SubscribeMessage("1号拉铆枪/产品完成");
            MqttClient.SubscribeMessage("2号拉铆枪/产品完成");
            MqttClient.SubscribeMessage("3号拉铆枪/产品完成");
            MqttClient.SubscribeMessage("4号拉铆枪/产品完成");
            // 伺服拉铆结果
            MqttClient.SubscribeMessage("1号拉铆枪/拉铆结果");
            MqttClient.SubscribeMessage("2号拉铆枪/拉铆结果");
            MqttClient.SubscribeMessage("3号拉铆枪/拉铆结果");
            MqttClient.SubscribeMessage("4号拉铆枪/拉铆结果");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ServoFinished 订阅失败");
        }
    }
    
    private static void MqttClient_OnMqttMessageReceived(MqttClient client, string topic, byte[] payload)
    {

        try
        {
            string message = Encoding.UTF8.GetString(payload);
            // Log.Information("收到伺服拉铆枪信号：" + topic + "  " + message);


            // 自动解析主题，提取设备号和事件类型
            var parts = topic.Split('/');
            if (parts.Length != 2)
            {
                LogUtil.ShowInMainPgae($"收到未知格式主题 {topic}: {message}");
                return;
            }

            string device = parts[0]; // 例如 "1号拉铆枪"
            string eventType = parts[1]; // 例如 "产品完成" 或 "拉铆结果"

            var payloadInt = Convert.ToInt32(Encoding.UTF8.GetString(payload));

            switch (eventType)
            {
                case "产品完成":
                    // 数量重置为0时触发产品完成
                    if (payloadInt == 0)
                    {
                        LogUtil.ShowInMainPgae($"[伺服{device}] 产品完成信号: {message}");
                        // ProductCompleted(int.Parse(device.Substring(0, device.IndexOf("号"))));
                        if (int.TryParse(device.Substring(0, device.IndexOf("号")), out int id))
                            ProductCompleted(id);
                    }
                    break;
                case "拉铆结果":
                    if (payloadInt != 0)
                    {
                        // LogUtil.ShowInMainPgae($"[伺服{device}] 拉铆结果信号: {message}");
                        Log.Information($"[伺服{device}] 拉铆结果信号: {message}");
                        // 提取设备编号，例如 "1号拉铆枪" -> id = 1
                        if (int.TryParse(device.Substring(0, device.IndexOf("号")), out int id))
                        {
                            // 异步延迟 100ms 后调用 ReadData
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(100); // 延迟 100ms
                                ReadData(id);
                            });
                        }


                    }
                    break;

                default:
                    Console.WriteLine($"[伺服{device}] 未知事件 {eventType}: {message}");
                    break;
            }
        }catch (Exception ex)
        {
            Log.Error("解析mqtt事件订阅失败：" + ex);
        }
    }
    
    // 拉铆结果
    private static void ReadData(int id)
    {
        try
        {
            OperateResult<JObject> read = MqttSyncClient.ReadRpc<JObject>("Edge/DeviceData", new { data = $"{id}号拉铆枪" });
            JObject jsonObj = JObject.Parse(read.Content.ToString());


            string d1 = (string?)jsonObj["每次拉完之后的拉力值"] ?? "";
            string d2 = (string?)jsonObj["每次拉完之后的行程值"] ?? "";
            string d3 = (string?)jsonObj["拉铆结果"] ?? "";
            
            
            string data = $"{d1},{d2},{d3}";

            // LogUtil.ShowInMainPgae("伺服拉铆完成信号: " + data + " id:" + id);
            
            Log.Information($"[伺服{id}号拉铆枪] 伺服拉铆完成信号: " + data);
            
            // 根据拉力值、行程值、拉铆结果做后续处理
            var rivetings = new Dictionary<int, ServoRivetingInstall>
            {
                { 1, ServoRivetingManage.ServoRiveting1 },
                { 2, ServoRivetingManage.ServoRiveting2 },
                { 3, ServoRivetingManage.ServoRiveting3 },
                { 4, ServoRivetingManage.ServoRiveting4 }
            };

            if (rivetings.TryGetValue(id, out var riveting))
            {
                if (riveting.Data != null)
                {
                    riveting.Data.Processes.Add(data);
                    LogUtil.AddLog($"[伺服{id}号拉铆枪]当前Data:" + riveting.Data);
                }
                else
                {
                    riveting.Data = new ServoRivetingData();
                    riveting.Data.Processes.Add(data);
                    LogUtil.AddLog($"[伺服{id}号拉铆枪]当前Data:" + riveting.Data);
                    // LogUtil.AddLog($"[伺服{id}号拉铆枪] Data为空");
                }
            }

        }
        catch (Exception ex)
        {
            LogUtil.ShowInMainPgae($"读取设备数据失败: {ex.Message}");
        }
    }
    
    // 产品完成
    private static void ProductCompleted(int id)
    { 
        LogUtil.ShowInMainPgae($"[伺服拉铆设备{id}]完成");
        try
        {
            switch (id)
            {
                case 1:
                    ServoRivetingManage.ServoRiveting1.SendToMes();
                    break;
                case 2:
                    ServoRivetingManage.ServoRiveting2.SendToMes();
                    break;
                case 3:
                    ServoRivetingManage.ServoRiveting3.SendToMes();
                    break;
                case 4:
                    ServoRivetingManage.ServoRiveting4.SendToMes();
                    break;
                default:
                    LogUtil.ShowInMainPgae($"未知伺服拉铆设备编号: {id}");
                    break;
                
            }
        }
        catch (Exception e)
        {
            LogUtil.ShowInMainPgae("伺服拉铆设备完成异常：" + e.Message);
        }
        
    }


}