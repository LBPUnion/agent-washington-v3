using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.Codex.Commands;

namespace LBPUnion.Codex;

[Plugin]
public class CodexPlugin : BotModule
{
    private SettingsManager _settings;
    private CommandManager _commands;
    
    protected override void BeforeInit()
    {
        Logger.Log("Codex is now initializing...");
        _settings = Modules.GetModule<SettingsManager>();
        _commands = Modules.GetModule<CommandManager>();
    }

    protected override void Init()
    {
        _commands.RegisterCommand<TestCommand>();
    }
}