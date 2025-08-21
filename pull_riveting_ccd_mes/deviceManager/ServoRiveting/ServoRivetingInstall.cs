using pull_riveting_ccd_mes.programUtil.log;
using pull_riveting_ccd_mes.util.mes;

namespace pull_riveting_ccd_mes.deviceManager.ServoRiveting;

public class ServoRivetingInstall : DeviceFather<ServoRivetingData>
{
    
    
    public ServoRivetingInstall(string name, string board, string userName, string process)
    { 
        Name = name;
        Board = board;
        UserName = userName;
        Processes = process;
        Data = new ServoRivetingData();
    }

}