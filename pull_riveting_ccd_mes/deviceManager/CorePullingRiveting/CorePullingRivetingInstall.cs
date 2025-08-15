namespace pull_riveting_ccd_mes.deviceManager.CorePullingRiveting;

public class CorePullingRivetingInstall : DeviceFather<CorePullingRivetingData>
{
    
    public CorePullingRivetingInstall(string name, string board, string userName)
    { 
        Name = name;
        Board = board;
        UserName = userName;
        Processes = "抽芯拉铆";
    }

}