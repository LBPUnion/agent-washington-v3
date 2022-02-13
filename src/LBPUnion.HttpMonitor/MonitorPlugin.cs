using System.Diagnostics;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Persistence;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.HttpMonitor.Commands;
using LBPUnion.HttpMonitor.Live;
using LBPUnion.HttpMonitor.Settings;

namespace LBPUnion.HttpMonitor;

[Plugin]
public class MonitorPlugin : BotModule
{
    // TODO: make this configurable
    private const double _updateInterval = 30;
    private double _timeUntilNextUpdate = 0;
    private DatabaseManager _database;
    private SettingsManager _settings;
    private CommandManager _commands;
    private MonitorSettingsProvider _monitorSettings;
    private Dictionary<string, MonitorStatus> _statuses = new Dictionary<string, MonitorStatus>();
    private LiveStatusScreen _liveStatus;

    protected override void BeforeInit()
    {
        Logger.Log("HTTP Monitor plugin is starting up...");

        _settings = Modules.GetModule<SettingsManager>();
        _commands = Modules.GetModule<CommandManager>();
    }

    protected override void Init()
    {
        Logger.Log("Registering monitor settings group...");
        _monitorSettings = _settings.RegisterSettings<MonitorSettingsProvider>();
        
        Logger.Log("Registering monitor commands...");

        _database = Modules.GetModule<DatabaseManager>();
        _commands.RegisterCommand<AddMonitorCommand>();
        _commands.RegisterCommand<RemoveMonitorCommand>();
        
        Logger.Log("Registering configurables for Monitor Plugin...");

        _settings.RegisterConfigurable("monitor.liveStatusChannel", new LiveStatusConfigurable());
        _settings.RegisterConfigurable("monitor.statusHistoryChannel", new StatusHistoryConfigurable());
        
    }

    internal void AddTarget(SocketGuild guild, MonitorTarget target)
    {
        _monitorSettings.AddTarget(guild.Id, target);
        _settings.SaveSettings();
    }

    internal bool TargetExists(SocketGuild guild, string name)
    {
        return _monitorSettings.TargetExistsInGuild(guild.Id, name);
    }

    private void UpdateMonitors()
    {
        if (_liveStatus == null)
        {
            var bot = Modules.GetModule<DiscordBot>();
            _liveStatus = new LiveStatusScreen(bot, _monitorSettings);
        }

        foreach (var guild in _monitorSettings.Guilds)
        {
            Logger.Log($"Performing server status monitor operation for guild {guild.Guild}...");
            var targetKeys = guild.Servers.Select(x => $"{guild.Guild.ToString()}:{x.Name}").ToArray();
            var badKeys = _statuses.Keys.Where(x => !targetKeys.Contains(x)).ToArray();

            foreach (var key in badKeys)
            {
                Logger.Log($"Server {key} was removed from the monitor list, it will no longer be monitored.");
                _statuses.Remove(key);
            }

            foreach (var target in guild.Servers)
            {
                var targetKey = $"{guild.Guild}:{target.Name}";
                if (!_statuses.ContainsKey(targetKey))
                {
                    Logger.Log($"New server {targetKey} will start being monitored now!");
                    _statuses.Add(targetKey, new MonitorStatus(_database, target));
                }

                Logger.Log($"Updating server: {targetKey}");
                var status = _statuses[targetKey];
                
                status.CheckStatus(_monitorSettings);

                if (status.HasStatusChanged)
                {
                    var bot = Modules.GetModule<DiscordBot>();
                    UpdateStatusHistoryAsync(bot, guild, status).Wait();
                }

                var liveServers = _statuses.Where(x => targetKeys.Contains(x.Key)).Select(x => x.Value);
                _liveStatus.UpdateStatus(guild, liveServers).Wait();
            }
            
            Logger.Log("Status check completed.");
        }
    }

    private async Task UpdateStatusHistoryAsync(DiscordBot bot, MonitorSettingsProvider.MonitorGuild guild, MonitorStatus status)
    {
        var actualGuild = bot.GetGuilds().FirstOrDefault(x => x.Id == guild.Guild);
        if (actualGuild != null)
        {
            if (actualGuild.GetChannel(guild.HistoryChannel) is SocketTextChannel channel)
            {
                var builder = new EmbedBuilder();
                builder.WithTitle($"{status.Name}: Server status changed!");
                builder.WithDescription($"`{status.Url}`");

                switch (status.ServerStatus)
                {
                    case ServerStatus.Offline:
                        builder.WithColor(Color.Red);
                        builder.AddField("Status", "Offline");
                        builder.AddField("Status Code", status.StatusCode);
                        break;
                    case ServerStatus.DnsError:
                        builder.WithColor(Color.Orange);
                        builder.AddField("Status", "Server Unreachable");
                        builder.AddField("Reason", "Agent Washington failed to resolve the hostname.");
                        break;
                    case ServerStatus.Unknown:
                        builder.WithColor(Color.Green);
                        builder.AddField("Status", "Online");
                        builder.AddField("Status Code", status.StatusCode);
                        break;
                    default:
                        builder.WithColor(Color.Orange);
                        builder.AddField("Status", "Unknown");
                        break;
                }

                await channel.SendMessageAsync(string.Empty, false, builder.Build());
            }
        }
    }
    
    protected override void OnTick(UpdateInterval interval)
    {
        _timeUntilNextUpdate -= interval.DeltaTime.TotalSeconds;
        if (_timeUntilNextUpdate <= 0)
        {
            UpdateMonitors();
            _timeUntilNextUpdate = _updateInterval;
        }
    }

    internal void DeleteTarget(SocketGuild guild, string targetName)
    {
        var target = _monitorSettings.Targets.FirstOrDefault(x => x.Name == targetName);
        if (target != null)
        {
            _monitorSettings.DeleteTarget(guild.Id, target);
            _settings.SaveSettings();
        }
    }
}