namespace LBPUnion.AgentWashington.Core.Settings;

public class ConfigurableAction : IConfigurable
{
    private Func<ConfigurableContext, string> _getter;
    private Action<ConfigurableContext, string> _setter;

    public ConfigurableAction(Func<ConfigurableContext, string> getter, Action<ConfigurableContext, string> setter)
    {
        _getter = getter;
        _setter = setter;
    }

    public string GetValue(ConfigurableContext ctx) => _getter(ctx);
    public void SetValue(ConfigurableContext ctx, string v) => _setter(ctx, v);
}