namespace LBPUnion.AgentWashington.Core.Settings;

public class ConfigurableAction : IConfigurable
{
    private Func<string> _getter;
    private Action<string> _setter;

    public ConfigurableAction(Func<string> getter, Action<string> setter)
    {
        _getter = getter;
        _setter = setter;
    }

    public string GetValue() => _getter();
    public void SetValue(string v) => _setter(v);
}