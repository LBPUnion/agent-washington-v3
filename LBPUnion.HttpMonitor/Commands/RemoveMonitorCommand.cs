using Discord;
using LBPUnion.AgentWashington.Core;

namespace LBPUnion.HttpMonitor.Commands;

public class RemoveMonitorCommand : Command
{
    public override string Name => "remove-server";
    public override string Description => "Remove a server from the server monitor list.";

    public override IEnumerable<Option> Options
    {
        get
        {
            yield return new Option("name", OptionType.String, "Name of the server to remove", true);
        }
    }

    protected override Task OnHandle()
    {
        var name = GetArgument<string>("name");
        var monitor = Modules.GetModule<MonitorPlugin>();

        if (monitor.TargetExists(name))
        {
            monitor.DeleteTarget(name);
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            embed.WithTitle($"Server deleted: {name}");
            embed.WithDescription(
                "This server will no longer be monitored, and will disappear from the live status shortly.");
            RespondWithEmbed(embed.Build());

        }
        else
        {
            var error = new EmbedBuilder();
            error.WithColor(Color.Red);
            error.WithTitle($"Can't remove server: {name}");
            error.WithDescription("No server with that name exists in the monitor list!");
            RespondWithEmbed(error.Build());
        }
        
        return Task.CompletedTask;
    }
}