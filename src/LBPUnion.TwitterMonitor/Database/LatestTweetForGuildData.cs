using LBPUnion.AgentWashington.Core.Persistence;

namespace LBPUnion.TwitterMonitor.Database; 

internal class LatestTweetForGuildData : IDatabaseObject {
    public Guid Id { get; set; }
    
    public ulong GuildId { get; set; }
    public ulong LatestTweetId { get; set; }
}