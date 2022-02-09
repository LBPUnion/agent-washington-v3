using System.Text.Json.Nodes;

namespace LBPUnion.AgentWashington.Core.Settings;

public interface ISettingsGroup
{
    void InitializeDefaults();
    void Serialize(JsonObject settingsData);
    void Deserialize(JsonObject settingsData);
}