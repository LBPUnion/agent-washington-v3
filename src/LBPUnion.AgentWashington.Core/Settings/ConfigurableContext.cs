using Discord.WebSocket;

namespace LBPUnion.AgentWashington.Core.Settings;

public class ConfigurableContext
{
    public SettingsManager Settings { get; }
    public SocketGuild Guild { get; }
    public SocketUser User { get; }

    internal ConfigurableContext(SettingsManager settings, SocketGuild guild, SocketUser user)
    {
        Settings = settings;
        Guild = guild;
        User = user;
    }
}