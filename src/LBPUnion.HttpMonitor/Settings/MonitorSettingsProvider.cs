using System.Text.Json;
using System.Text.Json.Nodes;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.HttpMonitor.Settings;

public class MonitorSettingsProvider : ISettingsGroup
{
    private List<MonitorTarget> _targets = new List<MonitorTarget>();
    private Dictionary<ulong, ulong> _liveStatusChannels = new Dictionary<ulong, ulong>();
    private Dictionary<ulong, ulong> _liveStatusMessages = new();
    private Dictionary<ulong, ulong> _historyChannels = new Dictionary<ulong, ulong>();

    public bool TryGetStatusHistoryChannel(ulong guild, out ulong channel)
    {
        return _historyChannels.TryGetValue(guild, out channel);
    }
    
    internal void SetLiveStatusChannelMessage(ulong channel, ulong message)
    {
        if (_liveStatusMessages.ContainsKey(channel))
            _liveStatusMessages[channel] = message;
        else
            _liveStatusMessages.Add(channel, message);
    }

    internal void SetLiveStatusGuildChannel(ulong guild, ulong channel)
    {
        if (_liveStatusChannels.ContainsKey(guild))
            _liveStatusChannels.Add(guild, channel);
        else
            _liveStatusChannels[guild] = channel;
    }
    
    public bool TryGetLiveStatusMessage(ulong channel, out ulong message)
    {
        return _liveStatusMessages.TryGetValue(channel, out message);
    }
    
    public bool TryGetGuildLiveStatusChannelId(ulong guild, out ulong channel)
    {
        return _liveStatusChannels.TryGetValue(guild, out channel);
    }
    
    internal IEnumerable<MonitorTarget> Targets => _targets;

    public void OnRegister(SettingsManager manager)
    {
        
    }

    public void InitializeDefaults()
    {
        _liveStatusChannels = new();
        _targets.Clear();
    }

    public void Serialize(JsonObject settingsData)
    {
        var array = new JsonArray();
        foreach (var target in _targets)
        {
            array.Add(target);
        }

        settingsData.Add("targets", array);

        var liveChannels = new JsonObject();

        foreach (var guild in _liveStatusChannels)
        {
            liveChannels.Add(guild.Key.ToString(), guild.Value.ToString());
        }
        
        settingsData.Add("liveChannels", liveChannels);
        
        var liveChannelMessages = new JsonObject();

        foreach (var guild in _liveStatusMessages)
        {
            liveChannelMessages.Add(guild.Key.ToString(), guild.Value.ToString());
        }
        
        settingsData.Add("liveChannelMessages", liveChannelMessages);
        
        var historyChannels = new JsonObject();

        foreach (var guild in _historyChannels)
        {
            historyChannels.Add(guild.Key.ToString(), guild.Value.ToString());
        }
        
        settingsData.Add("historyChannels", historyChannels);
    }

    public void Deserialize(JsonObject settingsData)
    {
        if (settingsData.TryGetPropertyValue("targets", out var targetsNode))
        {
            if (targetsNode is JsonArray array)
            {
                _targets.Clear();
                foreach (var elem in array)
                {
                    if (elem is JsonObject elemObject)
                    {
                        var target = elemObject.Deserialize<MonitorTarget>();
                        if (target != null)
                            _targets.Add(target);
                    }
                }
            }
        }

        if (settingsData.TryGetPropertyValue("liveChannels", out var node))
        {
            if (node is JsonObject liveChannels)
            {
                foreach (var key in liveChannels)
                {
                    if (ulong.TryParse(key.Key, out var guild)
                        && ulong.TryParse(key.Value?.GetValue<string>(), out var channel))
                    {
                        if (_liveStatusChannels.ContainsKey(guild))
                            _liveStatusChannels[guild] = channel;
                        else
                            _liveStatusChannels.Add(guild, channel);
                    }
                }
            }
        }
        
        if (settingsData.TryGetPropertyValue("liveChannelMessages", out node))
        {
            if (node is JsonObject liveChannels)
            {
                foreach (var key in liveChannels)
                {
                    if (ulong.TryParse(key.Key, out var guild)
                        && ulong.TryParse(key.Value?.GetValue<string>(), out var channel))
                    {
                        if (_liveStatusMessages.ContainsKey(guild))
                            _liveStatusMessages[guild] = channel;
                        else
                            _liveStatusMessages.Add(guild, channel);
                    }
                }
            }
        }
        
        if (settingsData.TryGetPropertyValue("historyChannels", out node))
        {
            if (node is JsonObject liveChannels)
            {
                foreach (var key in liveChannels)
                {
                    if (ulong.TryParse(key.Key, out var guild)
                        && ulong.TryParse(key.Value?.GetValue<string>(), out var channel))
                    {
                        if (_historyChannels.ContainsKey(guild))
                            _historyChannels[guild] = channel;
                        else
                            _historyChannels.Add(guild, channel);
                    }
                }
            }
        }
    }

    internal void AddTarget(MonitorTarget target)
    {
        _targets.Add(target);
    }

    internal void DeleteTarget(MonitorTarget target)
    {
        _targets.Remove(target);
    }
}