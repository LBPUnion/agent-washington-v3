using Discord;
using LBPUnion.AgentWashington.Core.Permissions;

namespace LBPUnion.AgentWashington.Core.Settings;

public class ConfigCommand : Command
{
    public override string Name => "config";
    public override string Description => "Configure Agent Washington.";

    protected override PermissionLevel MinimumPermissionLevel => PermissionLevel.Administrator;

    public override IEnumerable<Option> Options
    {
        get
        {
            yield return new Option("setting", OptionType.String, "The setting to update", true);
            yield return new Option("value", OptionType.String, "New value for the setting", true);
        }
    }

    protected override Task OnHandle()
    {
        this.MakeEphemeral();
        
        if (!this.TryGetArgument<string>("setting", out var setting))
        {
            var error = new EmbedBuilder();
            error.WithColor(Color.Red);
            error.WithTitle("Somehow, an error occurred.");
            error.WithDescription(
                "You did not supply a setting name to configure. This shouldn't have happened but we wrote this error message anyway.");
            RespondWithEmbed(error.Build());
            return Task.CompletedTask;
        }
        
        if (!this.TryGetArgument<string>("value", out var value))
        {
            var error = new EmbedBuilder();
            error.WithColor(Color.Red);
            error.WithTitle("Somehow, an error occurred.");
            error.WithDescription(
                "You did not supply a setting value to set. This shouldn't have happened but we wrote this error message anyway.");
            RespondWithEmbed(error.Build());
            return Task.CompletedTask;
        }

        var settings = this.Modules.GetModule<SettingsManager>();
        var ctx = new ConfigurableContext(settings, Guild, User);
        
        if (!settings.TryGetConfigurable(setting, out var configurable))
        {
            var error = new EmbedBuilder();
            error.WithColor(Color.Red);
            error.WithTitle("Configurable not found!");
            error.WithDescription($"No configurable setting named `{setting}` could be found.");
            RespondWithEmbed(error.Build());
            return Task.CompletedTask;
        }

        try
        {
            configurable.SetValue(ctx, value);
        }
        catch (Exception ex)
        {
            var error = new EmbedBuilder();
            error.WithColor(Color.Red);
            error.WithTitle("Cannot update configuration");
            error.WithDescription($"The following error has occurred: {ex.Message}");
            RespondWithEmbed(error.Build());
            return Task.CompletedTask;
        }

        settings.SaveSettings();
        
        var success = new EmbedBuilder();
        success.WithColor(Color.Green);
        success.WithTitle("Configuration updated!");
        success.WithDescription("The configuration has been saved.");
        RespondWithEmbed(success.Build());
        
        return Task.CompletedTask;
    }
}