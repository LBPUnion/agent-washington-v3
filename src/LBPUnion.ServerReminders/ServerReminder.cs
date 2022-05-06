using LBPUnion.AgentWashington.Core.Persistence;

namespace LBPUnion.ServerReminders;

public class ServerReminder : IDatabaseObject
{
    public int WeeksSinceLastUpdate { get; set; }
    public int WeeksBetweenUpdate { get; set; }
    public int Hour { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public string Text { get; set; }
    public bool HasBeenPostedThisHour { get; set; }
    public Guid Id { get; set; }
}