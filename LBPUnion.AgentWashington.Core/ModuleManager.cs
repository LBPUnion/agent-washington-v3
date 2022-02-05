using System.Diagnostics;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.AgentWashington.Core;

public class ModuleManager
{
    private bool _hasInitialized = false;
    private bool _hasBootstrapped = false;
    private List<BotModule> _modules = new List<BotModule>();

    public void RegisterModule(BotModule module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        if (_modules.Contains(module))
        {
            Logger.Log($"Module {module.Name} has already been registered. No need to re-register it.",
                LogLevel.Warning);
            return;
        }

        Logger.Log($"Registering module: {module.Name}");

        _modules.Add(module);

        if (_hasBootstrapped)
            module.Bootstrap(this);

        if (_hasInitialized)
            module.Initialize();
    }

    public T GetModule<T>() where T : BotModule
    {
        return _modules.OfType<T>().First();
    }

    public bool TryGetModule<T>(out T module) where T : BotModule
    {
        var mod = _modules.OfType<T>().FirstOrDefault();
        module = mod;
        return mod != null;
    }

    public void Bootstrap()
    {
        if (_hasBootstrapped)
        {
            Logger.Log("Bootstrap is already done.", LogLevel.Warning);
            return;
        }

        Logger.Log("Registering core modules...");
        RegisterModule(new SettingsManager());
        RegisterModule(new CommandManager());

        Logger.Log("Bootstrapping modules...");

        // Pass 1: Bootstrap each module. This links the module to the application and allows it to perform self-init operations.
        foreach (var module in _modules)
        {
            Logger.Log($"Bootstrapping module: {module.Name}");
            module.Bootstrap(this);
        }
        
        _hasBootstrapped = true;
        
        // Pass 2: All bootstrapping is done, time to let modules talk to each other.
        foreach (var mod in _modules)
        {
            Logger.Log($"Initializing: {mod.Name}");
            mod.Initialize();
        }

        _hasInitialized = true;
        
        // we must do this here since loading plugins during initialization will cause a crash.
        RegisterModule(new PluginManager());
        
        Logger.Log("Bootstrap completed.");
    }

    internal void Tick(UpdateInterval interval)
    {
        foreach (var plugin in _modules)
            plugin.Tick(interval);
    }
}