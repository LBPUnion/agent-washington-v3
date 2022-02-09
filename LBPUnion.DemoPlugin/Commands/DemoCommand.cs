using Discord;
using LBPUnion.AgentWashington.Core;

namespace LBPUnion.DemoPlugin.Commands;

public class DemoCommand : Command
{
    public override string Name => "demo-command";
    public override string Description => "This is a demo command defined in the demo plugin.";

    protected override Task OnHandle()
    {
        var builder = new EmbedBuilder().WithTitle("Demo Plugin")
            .WithDescription("This is the result of running the demo command inside the demo plugin.")
            .WithColor(Color.Green);

        this.RespondWithEmbed(builder.Build());
        
        return Task.CompletedTask;
    }
}