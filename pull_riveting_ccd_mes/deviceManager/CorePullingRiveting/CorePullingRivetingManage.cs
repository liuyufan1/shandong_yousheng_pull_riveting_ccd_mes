namespace pull_riveting_ccd_mes.deviceManager.CorePullingRiveting;

public class CorePullingRivetingManage
{
    public static CorePullingRivetingInstall MS11Left;
    public static CorePullingRivetingInstall MS11Right;
    public static CorePullingRivetingInstall MX11Left;
    public static CorePullingRivetingInstall MX11Right;
    
    public static void Init()
    { 
        MS11Left = new CorePullingRivetingInstall("拉铆1", "S1581", "拉铆1");
        MS11Right = new CorePullingRivetingInstall("拉铆2", "S1582", "拉铆2");
        MX11Left = new CorePullingRivetingInstall("拉铆3", "S1583", "拉铆3");
        MX11Right = new CorePullingRivetingInstall("拉铆4", "S1584", "拉铆4");
    }
}