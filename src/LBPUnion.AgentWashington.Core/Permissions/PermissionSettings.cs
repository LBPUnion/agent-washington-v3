using System.Text.Json;
using System.Text.Json.Nodes;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.AgentWashington.Core.Permissions;

public class PermissionSettings : ISettingsGroup
{
    private PermissionSettingsData _data = new PermissionSettingsData();

    public ulong DeveloperId => _data.DeveloperUserId;
    
    public void InitializeDefaults()
    {
        _data = new PermissionSettingsData();
    }

    public void Serialize(JsonObject settingsData)
    {
        settingsData.Add("Permissions", JsonSerializer.SerializeToNode(_data));
    }

    public void Deserialize(JsonObject settingsData)
    {
        if (settingsData.TryGetPropertyValue("Permissions", out var node))
        {
            if (node is JsonObject obj)
            {
                _data = obj.Deserialize<PermissionSettingsData>();
                if (_data == null)
                    InitializeDefaults();
            }
        }
    }

    internal bool TryGetGuildPermissions(ulong guild, out GuildPermissionData data)
    {
        var dataTest = _data.Guilds.FirstOrDefault(x => x.GuildId == guild);
        data = dataTest;
        return dataTest != null;
    }
    
    private class PermissionSettingsData
    {
        public ulong DeveloperUserId { get; set; }
        public List<GuildPermissionData> Guilds { get; set; } = new List<GuildPermissionData>();
    }

    internal class GuildPermissionData
    {
        public ulong GuildId { get; set; }
        public List<RoleData> Roles { get; set; } = new List<RoleData>();
    }

    internal class RoleData
    {
        public ulong RoleId { get; set; }
        public PermissionLevel PermissionLevel { get; set; }
    }
}