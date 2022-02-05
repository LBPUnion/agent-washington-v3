namespace LBPUnion.HttpMonitor.Settings;

public class MonitorTarget
{
    public string Name { get; set; }
    public bool UseSsl { get; set; }
    public bool IgnoreSslCertErrors { get; set; }
    public string Host { get; set; }
    public ushort Port { get; set; }
    public string Path { get; set; }
}