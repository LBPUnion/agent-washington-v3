using LBPUnion.AgentWashington.Core;

namespace LBPUnion.Codex.Commands;

public class TestCommand : Command
{
    public override string Name => "test";
    public override string Description => "This is a test command defined in the Codex plugin.";

    protected override Task OnHandle()
    {
        RespondWithText("Trixel Creative is cool.");
        
        return Task.CompletedTask;
    }
}