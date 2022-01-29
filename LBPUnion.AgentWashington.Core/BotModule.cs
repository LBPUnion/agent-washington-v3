using LBPUnion.AgentWashington.Core.Logging;

namespace LBPUnion.AgentWashington.Core;

public abstract class BotModule
{
    private bool _hasInitialized;
    private ModuleManager _moduleManager;
    
    public virtual string Name => GetType().Name;

    protected ModuleManager Modules => _moduleManager;
    
    internal void Bootstrap(ModuleManager moduleManager)
    {
        if (_moduleManager != null)
            throw new InvalidOperationException("Module has already been bootstrapped.");

        if (moduleManager == null)
            throw new ArgumentNullException(nameof(moduleManager));

        _moduleManager = moduleManager;

        BeforeInit();
    }

    internal void Initialize()
    {
        if (_hasInitialized)
        {
            Logger.Log($"{Name} has already initialized.", LogLevel.Warning);
            return;
        }

        Init();
        _hasInitialized = true;
    }
    
    protected virtual void BeforeInit() {}
    protected virtual void Init() {}
}