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

    public async Task UpdateStatus(IEnumerable<MonitorStatus> monitors)
    {
        var embed = new EmbedBuilder();
        embed.WithColor(Color.Blue);
        embed.WithTitle("Server Status");
        embed.WithDescription("Current status of all monitored servers.");
        embed.WithFooter($"Last checked: {DateTime.UtcNow} (UTC)");
        
        foreach (var monitor in monitors)
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
        
        foreach (var guild in _bot.GetGuilds())
        {
            if (_settings.TryGetGuildLiveStatusChannelId(guild.Id, out var channelId))
            {
                try
                {
                    var channel = guild.GetTextChannel(channelId);

                    if (_settings.TryGetLiveStatusMessage(channelId, out var messageId))
                    {
                        var message = await channel.GetMessageAsync(messageId);
                        var m88youngling = message as RestUserMessage;
                        if (m88youngling != null)
                        {
                            await m88youngling.ModifyAsync((edit) => { edit.Embed = embed.Build(); });
                            continue;
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
    }
}