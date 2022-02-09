﻿using System.Text.Json;
using System.Text.Json.Nodes;
using LBPUnion.AgentWashington.Core.Logging;

namespace LBPUnion.AgentWashington.Core.Settings;

public class SettingsManager : BotModule
{
    private List<ISettingsGroup> _groups = new List<ISettingsGroup>();
    private string _settingsPath;

    public T RegisterSettings<T>() where T : ISettingsGroup, new()
    {
        var group = new T();
        Logger.Log($"Registering settings group: {group.GetType().Name}");
        _groups.Add(group);

        ReloadSettings();
        
        return group;
    }

    protected override void BeforeInit()
    {
        _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        Logger.Log($"Settings path is {_settingsPath}");
    }

    protected override void Init()
    {
        ReloadSettings();
    }

    private void ReloadSettings()
    {
        Logger.Log("Re-loading configuration...");

        if (File.Exists(_settingsPath))
        {
            var json = File.ReadAllText(_settingsPath);

            try
            {
                var jobj = JsonNode.Parse(json) as JsonObject;

                if (jobj == null)
                    throw new InvalidOperationException(
                        "Settings deserialized to a valid JSON value, but did not deserialize to a JSON object.");

                var shouldSave = false;
                
                foreach (var group in _groups)
                {
                    var name = group.GetType().Name;

                    if (jobj.TryGetPropertyValue(name, out var groupNode))
                    {
                        if (groupNode is JsonObject groupObject)
                        {
                            group.Deserialize(groupObject);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"{name}: JSON serialized to a valid JSON value, but did not serialize to a JSON object.");
                        }
                    }
                    else
                    {
                        Logger.Log($"{name}: No JSON found, using default settings. Settings will be saved.");
                        group.InitializeDefaults();
                        shouldSave = true;
                    }
                }

                if (shouldSave)
                    SaveSettings();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while loading settings: {ex.Message} - Defaults will be used.", LogLevel.Warning);
                InitializeDefaults();
            }
        }
        else
        {
            Logger.Log("Settings file not found - defaults will be used, and a new settings file will be saved.");
            InitializeDefaults();

            SaveSettings();
        }
    }

    public void SaveSettings()
    {
        Logger.Log("Saving configuration...");

        var rootObject = new JsonObject();

        foreach (var group in _groups)
        {
            var groupObject = new JsonObject();

            group.Serialize(groupObject);

            rootObject.Add(group.GetType().Name, groupObject);
        }

        var json = rootObject.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(_settingsPath, json);
    }
    
    private void InitializeDefaults()
    {
        Logger.Log("Initializing settings...");

        foreach (var group in _groups)
        {
            group.InitializeDefaults();
        }
    }

}