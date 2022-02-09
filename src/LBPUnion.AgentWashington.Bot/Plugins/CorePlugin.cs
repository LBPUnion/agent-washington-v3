using LBPunion.AgentWashington.Bot.Commands;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Plugins;

namespace LBPunion.AgentWashington.Bot.Plugins;

[Plugin]
public class CorePlugin : BotModule
{
    private CommandManager _commands;
    
    protected override void BeforeInit()
    {
        _commands = Modules.GetModule<CommandManager>();
    }

    protected override void Init()
    {
        _commands.RegisterCommand<HelloCommand>();
    }
}