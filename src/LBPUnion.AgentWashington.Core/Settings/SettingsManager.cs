using System.Text.Json;
using System.Text.Json.Nodes;
using LBPUnion.AgentWashington.Core.Logging;

namespace LBPUnion.AgentWashington.Core.Settings;

public class SettingsManager : BotModule
{
    private CommandManager _commands;
    private List<ISettingsGroup> _groups = new List<ISettingsGroup>();
    private string _settingsPath;
    private Dictionary<string, IConfigurable> _configurables = new();

    public bool TryGetConfigurable(string id, out IConfigurable configurable)
    {
        return _configurables.TryGetValue(id, out configurable);
    }
    
    public T RegisterSettings<T>() where T : ISettingsGroup, new()
    {
        var ofType = _groups.OfType<T>().FirstOrDefault();
        if (ofType != null)
            return ofType;
        
        var group = new T();
        Logger.Log($"Registering settings group: {group.GetType().Name}");
        _groups.Add(group);

        group.OnRegister(this);
        ReloadSettings();
        
        return group;
    }

    protected override void BeforeInit()
    {
        _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        _commands = Modules.GetModule<CommandManager>();
        Logger.Log($"Settings path is {_settingsPath}");
    }

    protected override void Init()
    {
        ReloadSettings();
        _commands.RegisterCommand<ConfigCommand>();
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

    public void RegisterConfigurable(string id, IConfigurable configurable)
    {
        _configurables.Add(id, configurable);
    }

    public void RegisterConfigurable(string id, Func<string> getter, Action<string> setter)
    {
        RegisterConfigurable(id, new ConfigurableAction(getter, setter));
    }
}