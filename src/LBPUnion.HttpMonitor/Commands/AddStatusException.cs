using Discord;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Permissions;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.HttpMonitor.Settings;

namespace LBPUnion.HttpMonitor.Commands;

public class AddStatusException : Command
{
    public override string Name => "add-error-exception";

    public override string Description =>
        "Add an error code exception rule to a server. This will treat the error as 'online'.";

    protected override PermissionLevel MinimumPermissionLevel => PermissionLevel.Administrator;

    public override IEnumerable<Option> Options
    {
        get
        {
            yield return new Option("name", OptionType.String, "The server whose configuration should be modified.", true);
            yield return new Option("http-status", OptionType.Integer, "The HTTP status code to treat as success.",
                true);
        }
    }

    protected override Task OnHandle()
    {
        var name = GetArgument<string>("name");
        var statusCode = (int) GetArgument<long>("http-status");

        var monitor = Modules.GetModule<MonitorPlugin>();
        var settings = Modules.GetModule<SettingsManager>();
        var monitorSettings = settings.RegisterSettings<MonitorSettingsProvider>();
        
        if (!monitor.TargetExists(Guild, name))
        {
            var err = new EmbedBuilder();
            err.WithColor(Color.Red);
            err.WithTitle("Server doesn't exist!");
            err.WithDescription($"The server `{name}` has not been added as a monitor target.");

            RespondWithEmbed(err.Build());
            return Task.CompletedTask;
        }

        var targetData = monitorSettings.Guilds.First(x => x.Guild == this.Guild.Id).Servers.First(x => x.Name == name);
        if (!targetData.ErrorCodeExceptions.Contains(statusCode))
        {
            targetData.ErrorCodeExceptions.Add(statusCode);

            settings.SaveSettings();

            monitor.ForceUpdate();
        }

        var embed = new EmbedBuilder();
        embed.WithColor(Color.Green);
        embed.WithTitle("Configuration updated!");
        embed.WithDescription("The server status has also been refreshed.");
        RespondWithEmbed(embed.Build());

        return Task.CompletedTask;
    }
}