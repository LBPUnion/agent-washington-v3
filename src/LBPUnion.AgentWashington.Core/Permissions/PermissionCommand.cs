using Discord;

namespace LBPUnion.AgentWashington.Core.Permissions;

public class PermissionCommand : Command
{
    public override string Name => "my-permissions";
    public override string Description => "Displays your bot permission level based on your roles in this server.";

    protected override Task OnHandle()
    {
        MakeEphemeral();
        
        if (!WasRunInGuild)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Cannot run command");
            embed.WithDescription("This command can only be run inside of a Discord server.");
            embed.WithColor(Color.Red);
            RespondWithEmbed(embed.Build());
            return Task.CompletedTask;
        }
        
        var permissions = Modules.GetModule<PermissionManager>();
        var user = this.User;

        var permissionLevel = permissions.GetPermissionLevel(user);

        RespondWithText($"Your current permission level is {permissionLevel}.");
        return Task.CompletedTask;
    }
}