using System.Text.Json;
using System.Text.Json.Nodes;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.TwitterMonitor.Settings; 

public class TwitterSettingsProvider : ISettingsGroup {
    private TwitterSettingsData _data = new();
    
    public void OnRegister(SettingsManager manager) {
        
    }
    
    public void InitializeDefaults() {
        this._data = new TwitterSettingsData();
    }
    
    public void Serialize(JsonObject settingsData) {
        settingsData.Add("twitterSettings", JsonSerializer.SerializeToNode(_data));
    }
    
    public void Deserialize(JsonObject settingsData) {
        if(settingsData.TryGetPropertyValue("twitterSettings", out var node)) {
            if(node is JsonObject obj) {
                _data = obj.Deserialize<TwitterSettingsData>()!;
            }
        }
    }

    private TwitterMonitorGuild getOrCreateGuild(ulong guildId) {
        TwitterMonitorGuild? guild = this._data.Guilds.FirstOrDefault(g => g.GuildId == guildId);
        if(guild == null) {
            guild = new TwitterMonitorGuild {
                GuildId = guildId,
            };

            this._data.Guilds.Add(guild);
        }

        return guild;
    }

    public List<TwitterMonitorGuild> GetGuilds() {
        return this._data.Guilds;
    }

    public bool TryGetBearerToken(out string token) {
        token = string.Empty;
        
        if(this._data.BearerToken == null) return false;

        token = this._data.BearerToken;
        return true;
    }

    public bool TryGetTwitterUserId(ulong guildId, out ulong userId) {
        userId = 0;

        TwitterMonitorGuild? guild = this._data.Guilds.FirstOrDefault(g => g.GuildId == guildId); 
        if(guild != null) {
            if(guild.TwitterUserId == null) return false;

            userId = (ulong)guild.TwitterUserId;
            return true;
        }
        return false;
    }
    
    public void SetTwitterUserId(ulong guildId, ulong userId) {
        TwitterMonitorGuild guild = getOrCreateGuild(guildId);
        guild.TwitterUserId = userId;
    }

    public bool TryGetUpdateChannelId(ulong guildId, out ulong channelId) {
        channelId = 0;

        TwitterMonitorGuild? guild = this._data.Guilds.FirstOrDefault(g => g.GuildId == guildId);
        if(guild != null) {
            if(guild.UpdateChannelId == null) return false;

            channelId = (ulong)guild.UpdateChannelId;
            return true;
        }
        return false;
    }

    public void SetUpdateChannelId(ulong guildId, ulong channelId) {
        TwitterMonitorGuild guild = getOrCreateGuild(guildId);
        guild.UpdateChannelId = channelId;
    }
}