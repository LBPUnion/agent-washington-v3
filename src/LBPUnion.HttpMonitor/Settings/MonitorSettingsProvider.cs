using System.Text.Json;
using System.Text.Json.Nodes;
using Discord;
using LBPUnion.AgentWashington.Core.Persistence;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.HttpMonitor.Settings;

public class MonitorSettingsProvider : ISettingsGroup
{
    private DatabaseManager _db;
    private MonitorSettingsData _data = new();
    
    public bool TryGetStatusHistoryChannel(ulong guild, out ulong channel)
    {
        channel = 0;
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData == null)
            return false;

        channel = guildData.HistoryChannel;
        return true;
    }

    public bool ShouldHideURLs(ulong guild)
    {
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData == null) return false;
        return guildData.ShouldHideUrls;
    }

    public void SetShouldHideUrls(ulong guild, bool value)
    {
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData == null)
        {
            guildData = new MonitorGuild() {Guild = guild};
            _data.Guilds.Add(guildData);
        }

        guildData.ShouldHideUrls = value;
    }

    internal IEnumerable<MonitorGuild> Guilds => _data.Guilds;

    public bool TargetExistsInGuild(ulong guildId, string name)
    {
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guildId);
        if (guildData == null)
            return false;

        return guildData.Servers.Any(x => x.Name == name);
    }

    internal void SetLiveStatusChannelMessage(ulong channel, ulong message)
    {
        _db.OpenDatabase((db) =>
        {
            var maps = db.GetCollection<LiveStatusMessageMap>("liveStatusChannels");

            var map = maps.FindOne(x => x.Channel == channel.ToString());
            if (map != null)
            {
                map.Message = message.ToString();
                maps.Update(map);
            }
            else
            {
                map = new LiveStatusMessageMap();
                map.Channel = channel.ToString();
                map.Message = message.ToString();
                maps.Insert(map);
            }
        });
    }

    internal void SetLiveStatusGuildChannel(ulong guild, ulong channel)
    {
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData == null)
        {
            guildData = new MonitorGuild() {Guild = guild};
            _data.Guilds.Add(guildData);
        }

        guildData.LiveStatusChannel = channel;
    }

    internal void SetStatusHistoryChannel(ulong guild, ulong channel)
    {
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData == null)
        {
            guildData = new MonitorGuild() {Guild = guild};
            _data.Guilds.Add(guildData);
        }

        guildData.HistoryChannel = channel;
    }

    public bool TryGetLiveStatusMessage(ulong channel, out ulong message)
    {
        var msg = 0ul;

        _db.OpenDatabase(db =>
        {
            var maps = db.GetCollection<LiveStatusMessageMap>("liveStatusChannels");
            var map = maps.FindOne(x => x.Channel == channel.ToString());
            if (map != null)
                msg = ulong.Parse(map.Message);
        });
        
        message = msg;
        return message != 0;
    }
    
    public bool TryGetGuildLiveStatusChannelId(ulong guild, out ulong channel)
    {
        channel = 0;
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData == null)
            return false;

        channel = guildData.LiveStatusChannel;
        return true;
    }

    internal IEnumerable<MonitorTarget> Targets => _data.Guilds.SelectMany(guild => guild.Servers);
    

    public void OnRegister(SettingsManager manager)
    {
        _db = manager.Modules.GetModule<DatabaseManager>();
    }

    public void InitializeDefaults()
    {
        _data = new();
    }

    public void Serialize(JsonObject settingsData)
    {
        settingsData.Add("monitorSettings", JsonSerializer.SerializeToNode(_data));
    }

    public void Deserialize(JsonObject settingsData)
    {
        if (settingsData.TryGetPropertyValue("monitorSettings", out var node))
        {
            if (node is JsonObject obj)
            {
                _data = obj.Deserialize<MonitorSettingsData>();
            }
        }
    }

    internal void AddTarget(ulong guild, MonitorTarget target)
    {
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData == null)
        {
            guildData = new MonitorGuild() {Guild = guild};
            _data.Guilds.Add(guildData);
        }

        guildData.Servers.Add(target);
    }

    internal void DeleteTarget(ulong guild, MonitorTarget target)
    {
        var guildData = _data.Guilds.FirstOrDefault(x => x.Guild == guild);
        if (guildData != null)
        {
            if (guildData.Servers.Contains(target))
                guildData.Servers.Remove(target);
        }
    }

    private class MonitorSettingsData
    {
        public List<MonitorGuild> Guilds { get; set; } = new();
    }

    internal class MonitorGuild
    {
        public bool ShouldHideUrls { get; set; }
        public ulong Guild { get; set; }
        public List<MonitorTarget> Servers { get;set; } = new();
        public ulong LiveStatusChannel { get; set; }
        public ulong HistoryChannel { get; set; }
    }

    private class LiveStatusMessageMap : IDatabaseObject
    {
        public Guid Id { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
    }
}