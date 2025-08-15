namespace pull_riveting_ccd_mes.deviceManager.ServoRiveting;

public class ServoRivetingData
{
    public List<string> Processes = new();

    public override string ToString()
    {
        return string.Join("，" + Processes);
    }
}