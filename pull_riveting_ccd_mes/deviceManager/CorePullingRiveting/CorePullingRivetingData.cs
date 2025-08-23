namespace pull_riveting_ccd_mes.deviceManager.CorePullingRiveting;

public class CorePullingRivetingData : DataFather
{
    public int Number { get; set; }

    public override string ToString()
    {
        return Number.ToString();
    }
}