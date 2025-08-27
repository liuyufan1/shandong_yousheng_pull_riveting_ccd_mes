namespace pull_riveting_ccd_mes.util.scanner;

/// <summary>
/// PDA 扫码接收类
/// </summary>
public class PdaBarcodeEntity
{
    public string barcode { get; set; }
    public string machineName { get; set; }
    public bool isReplenishment { get; set; }
}