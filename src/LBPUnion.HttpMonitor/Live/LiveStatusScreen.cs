using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.HttpMonitor.Settings;

namespace LBPUnion.HttpMonitor.Live;

public class LiveStatusScreen
{
    private DiscordBot _bot;
    private MonitorSettingsProvider _settings;

    internal LiveStatusScreen(DiscordBot bot, MonitorSettingsProvider settings)
    {
        _bot = bot;
        _settings = settings;
    }

    internal async Task UpdateStatus(MonitorSettingsProvider.MonitorGuild guild, IEnumerable<MonitorStatus> servers)
    {
        if (guild.LiveStatusChannel == 0)
            return;

        var actualGuild = _bot.GetGuilds().FirstOrDefault(x => x.Id == guild.Guild);
        if (actualGuild == null)
            return;
        
        var embed = new EmbedBuilder();
        embed.WithColor(Color.Blue);
        embed.WithTitle("Server Status");
        embed.WithDescription("Current status of all monitored servers.");
        embed.WithFooter($"Last checked: {DateTime.UtcNow} (UTC)");

        foreach (var monitor in servers)
        {
            var emoji = monitor.ServerStatus switch
            {
                ServerStatus.Online => ":white_check_mark:",
                ServerStatus.Offline => ":no_entry_sign:",
                ServerStatus.DnsError => ":no_entry_sign:",
                ServerStatus.Unknown => ":question:"
            };

            var fieldText = monitor.ServerStatus switch
            {
                ServerStatus.Online => "Up!",
                ServerStatus.Offline => monitor.StatusCode.ToString(),
                ServerStatus.DnsError => "DNS Error",
                ServerStatus.Unknown => "Unknown"
            };

            embed.AddField(monitor.Name, $"{emoji} {fieldText}");
        }


        try
        {
            var channelId = guild.LiveStatusChannel;
            var channel = actualGuild.GetTextChannel(channelId);

            if (_settings.TryGetLiveStatusMessage(channelId, out var messageId))
            {
                var message = await channel.GetMessageAsync(messageId);
                var m88youngling = message as RestUserMessage;
                if (m88youngling != null)
                {
                    await m88youngling.ModifyAsync((edit) => { edit.Embed = embed.Build(); });
                    return;
                }
            }

            var newMessage = await channel.SendMessageAsync(string.Empty, false, embed.Build());
            _settings.SetLiveStatusChannelMessage(channelId, newMessage.Id);
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message, LogLevel.Error);
        }
    }
}