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

    public bool TryGetBearerToken(out string token) {
        token = string.Empty;
        
        if(this._data.BearerToken == null) return false;

        token = this._data.BearerToken;
        return true;
    }

    public bool TryGetTwitterUserId(out ulong userId) {
        userId = 0;

        if(this._data.UserId == null) return false;

        userId = (ulong)this._data.UserId;
        return true;
    }
    
    public void SetTwitterUserId(ulong userId) {
        this._data.UserId = userId;
    }
}