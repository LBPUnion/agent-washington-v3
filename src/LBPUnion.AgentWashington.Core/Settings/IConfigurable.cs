namespace LBPUnion.AgentWashington.Core.Settings;

public interface IConfigurable
{
    string GetValue();
    void SetValue(string value);
}