using LBPUnion.AgentWashington.Core;

namespace LBPunion.AgentWashington.Bot.Commands;

public class HelloCommand : Command
{
    public override string Name => "hello";
    public override string Description => "Test to see if the bot is working by saying hello.";
    protected override Task OnHandle()
    {
        RespondWithText("Hello! I'm alive!");

        return Task.CompletedTask;
    }
}