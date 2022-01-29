using System.Diagnostics;
using System.Reflection;
using LBPUnion.AgentWashington.Core.Logging;

namespace LBPUnion.AgentWashington.Core.Plugins;

public class PluginManager : BotModule
{
    private List<BotModule> _plugins = new List<BotModule>();
    private string _pluginsDirectory;
    
    protected override void BeforeInit()
    {
        _pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        Logger.Log($"Plugins directory is {_pluginsDirectory}");

        if (!Directory.Exists(_pluginsDirectory))
        {
            Directory.CreateDirectory(_pluginsDirectory);
            Logger.Log("Plugins directory has been created.");
        }

        LoadPlugins();
    }

    protected override void Init()
    {
        foreach (var module in _plugins)
        {
            Modules.RegisterModule(module);
        }
    }

    private void LoadPlugins()
    {
        Logger.Log("Loading plugins...");

        foreach (var file in Directory.GetFiles(_pluginsDirectory))
        {
            if (!file.ToLower().EndsWith(".dll"))
                continue;

            try
            {
                var assembly = Assembly.LoadFrom(file);
                Logger.Log($"Loaded {assembly.FullName} from {file}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Cannot load plugin at path {file} - {ex.Message}", LogLevel.Error);
            }
        }

        Logger.Log("Finding all plugins in loaded assemblies...");

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var botModuleType = typeof(BotModule);
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAssignableTo(botModuleType))
                    continue;

                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                    continue;

                var pluginAttribute = type.GetCustomAttributes(false).OfType<PluginAttribute>().Any();
                if (!pluginAttribute)
                    continue;

                var module = Activator.CreateInstance(type, null) as BotModule;
                if (module == null)
                    continue;

                Logger.Log($"Loaded module {module.Name} from {type.FullName} in assembly {assembly.FullName}");

                _plugins.Add(module);
            }
        }
    }
}