using System.Diagnostics;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
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
        
        _commands.RegisterCommand<AddMonitorCommand>();
        _commands.RegisterCommand<RemoveMonitorCommand>();
    }

    internal void AddTarget(MonitorTarget target)
    {
        _monitorSettings.AddTarget(target);
        _settings.SaveSettings();
    }

    internal bool TargetExists(string name)
    {
        return _monitorSettings.Targets.Any(x => x.Name == name);
    }

    private void UpdateMonitors()
    {
        if (_liveStatus == null)
        {
            var bot = Modules.GetModule<DiscordBot>();
            _liveStatus = new LiveStatusScreen(bot, _monitorSettings);
        } 
        
        Logger.Log("Performing server status monitor operation...");
        var badKeys = _statuses.Keys.Where(x => _monitorSettings.Targets.All(y => y.Name != x)).ToArray();

        foreach (var key in badKeys)
        {
            Logger.Log($"Server {key} was removed from the monitor list, it will no longer be monitored.");
            _statuses.Remove(key);
        }

        foreach (var target in _monitorSettings.Targets)
        {
            if (!_statuses.ContainsKey(target.Name))
            {
                Logger.Log($"New server {target.Name} will start being monitored now!");
                _statuses.Add(target.Name, new MonitorStatus(target));
            }
        }

        foreach (var status in _statuses)
        {
            Logger.Log($"Updating server: {status.Key}");
            status.Value.CheckStatus();
        }

        Logger.Log("Status check completed.");

        _liveStatus.UpdateStatus(_statuses.Values);
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

    internal void DeleteTarget(string targetName)
    {
        var target = _monitorSettings.Targets.FirstOrDefault(x => x.Name == targetName);
        if (target != null)
        {
            _monitorSettings.DeleteTarget(target);
            _settings.SaveSettings();
        }
    }
}