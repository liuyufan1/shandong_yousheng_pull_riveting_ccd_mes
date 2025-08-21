namespace pull_riveting_ccd_mes.deviceManager.ccd;

public interface CCDManage
{
    public static CCDInstall MX11CCD;
    public static CCDInstall MS11CCD;

    public static void Init()
    {
        MX11CCD = new CCDInstall("CCD1", "S1585", "CCD1", "CCD1");
        MS11CCD = new CCDInstall("CCD2", "S1586", "CCD2", "CCD2");
    }
}