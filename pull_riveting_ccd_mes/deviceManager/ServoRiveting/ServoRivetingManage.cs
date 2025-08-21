namespace pull_riveting_ccd_mes.deviceManager.ServoRiveting;

public class ServoRivetingManage
{
    public static ServoRivetingInstall ServoRiveting1;
    public static ServoRivetingInstall ServoRiveting2;
    public static ServoRivetingInstall ServoRiveting3;
    public static ServoRivetingInstall ServoRiveting4;
    
    public static void Init()
    { 
        ServoRiveting1 = new ServoRivetingInstall("伺服拉铆1", "S1587", "伺服拉铆1", "伺服拉铆1");
        ServoRiveting2 = new ServoRivetingInstall("伺服拉铆2", "S1588", "伺服拉铆2", "伺服拉铆2");
        ServoRiveting3 = new ServoRivetingInstall("伺服拉铆3", "S1589", "伺服拉铆3", "伺服拉铆3");
        ServoRiveting4 = new ServoRivetingInstall("伺服拉铆4", "S1590", "伺服拉铆4", "伺服拉铆4");
        
    }
    
}