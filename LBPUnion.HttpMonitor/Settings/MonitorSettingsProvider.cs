using System.Text.Json;
using System.Text.Json.Nodes;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.HttpMonitor.Settings;

public class MonitorSettingsProvider : ISettingsGroup
{
    private List<MonitorTarget> _targets = new List<MonitorTarget>();

    public void InitializeDefaults()
    {
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
    }
}