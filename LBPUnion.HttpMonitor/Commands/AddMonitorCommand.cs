using LBPUnion.AgentWashington.Core;

namespace LBPUnion.HttpMonitor.Commands;

public class AddMonitorCommand : Command
{
    public override string Name => "add-server";
    public override string Description => "Add a new server to the Server Monitor list.";

    protected override Task OnHandle()
    {
        return Task.CompletedTask;
    }
}