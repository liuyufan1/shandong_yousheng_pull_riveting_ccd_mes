namespace pull_riveting_ccd_mes.deviceManager.CorePullingRiveting;

public class CorePullingRivetingInstall : DeviceFather<CorePullingRivetingData>
{
    
    public CorePullingRivetingInstall(string name, string board, string userName, string process)
    { 
        Name = name;
        Board = board;
        UserName = userName;
        Processes = process;
    }

}