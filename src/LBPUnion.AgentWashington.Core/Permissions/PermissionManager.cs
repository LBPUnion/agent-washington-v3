using System.Reflection;
using Discord.WebSocket;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.AgentWashington.Core.Permissions;

public class PermissionManager : BotModule
{
    private SettingsManager _settings;
    private PermissionSettings _permSettings;
    private CommandManager _commands;
    
    protected override void BeforeInit()
    {
        Logger.Log("PermissionManager is starting up.");

        _settings = Modules.GetModule<SettingsManager>();
        _commands = Modules.GetModule<CommandManager>();
    }

    protected override void Init()
    {
        _permSettings = _settings.RegisterSettings<PermissionSettings>();
        _commands.RegisterCommand<PermissionCommand>();
    }
    
    public PermissionLevel GetPermissionLevel(SocketUser user)
    {
        var developerId = _permSettings.DeveloperId;
        if (user.Id == developerId)
            return PermissionLevel.Developer;
        
        // Check guild permissions.
        if (user is SocketGuildUser guildMember)
        {
            // Guild ID.
            var guild = guildMember.Guild.Id;
            
            // If the user has the Discord "Administrator" permission, treat them as an admin.
            if (guildMember.GuildPermissions.Administrator)
                return PermissionLevel.Administrator;
            
            // Retrieve the guild's permission table.
            if (_permSettings.TryGetGuildPermissions(guild, out var guildPerms))
            {
                // Grab the user's roles.
                var roles = guildMember.Roles;
                
                // Grab role data for the user's roles, and order by the permission level.
                var rolePermissions = guildPerms.Roles.Where(x => roles.Any(y => y.Id == x.RoleId))
                    .OrderByDescending(x => (int) x.PermissionLevel);
                
                // Get the first role, that'll be the highest permission for the user. If we get null, then consider the user as having default permissions.
                var highestRole = rolePermissions.FirstOrDefault();
                if (highestRole != null)
                    return highestRole.PermissionLevel;
            }
        }
        
        // Default permissions.
        return PermissionLevel.Default;
    }
}