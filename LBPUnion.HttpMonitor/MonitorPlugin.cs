using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.HttpMonitor.Commands;
using LBPUnion.HttpMonitor.Settings;

namespace LBPUnion.HttpMonitor;

[Plugin]
public class MonitorPlugin : BotModule
{
    private SettingsManager _settings;
    private CommandManager _commands;
    private MonitorSettingsProvider _monitorSettings;
    
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
    }
}