namespace LBPUnion.AgentWashington.Core.Settings;

public interface IConfigurable
{
    string GetValue(ConfigurableContext ctx);
    void SetValue(ConfigurableContext ctx, string value);
}