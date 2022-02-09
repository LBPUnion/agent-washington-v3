using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.DemoPlugin.Commands;

namespace LBPUnion.DemoPlugin;

[Plugin]
public class DemoPlugin : BotModule
{
    private CommandManager _command;
    
    protected override void BeforeInit()
    {
        Logger.Log("Demo plugin is initializing!");
        _command = Modules.GetModule<CommandManager>();
    }

    protected override void Init()
    {
        _command.RegisterCommand<DemoCommand>();
    }
}