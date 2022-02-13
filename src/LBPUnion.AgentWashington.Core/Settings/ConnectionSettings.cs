using System.Text.Json.Nodes;

namespace LBPUnion.AgentWashington.Core.Settings;

public class ConnectionSettings : ISettingsGroup
{
    private string _token;

    internal string Token => _token;
    public void OnRegister(SettingsManager manager)
    {
        
    }

    public void InitializeDefaults()
    {
        _token = string.Empty;
    }

    public void Serialize(JsonObject settingsData)
    {
        settingsData.Add("Token", JsonValue.Create(_token));
    }

    public void Deserialize(JsonObject settingsData)
    {
        if (settingsData.TryGetPropertyValue("Token", out var tokenValue))
        {
            if (tokenValue is JsonValue value)
            {
                _token = value.GetValue<string>();
            }
        }
    }
}