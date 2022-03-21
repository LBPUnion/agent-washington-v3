namespace LBPUnion.TwitterMonitor.Settings; 

public class TwitterMonitorGuild {
    public ulong GuildId { get; init; }
    public ulong? UpdateChannelId { get; set; }
    public ulong? TwitterUserId { get; set; }
}