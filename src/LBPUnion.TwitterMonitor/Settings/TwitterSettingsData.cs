namespace LBPUnion.TwitterMonitor.Settings; 

public class TwitterSettingsData {
    public string? BearerToken { get; set; }

    public List<TwitterMonitorGuild> Guilds { get; set; } = new();
}