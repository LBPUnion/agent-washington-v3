namespace LBPUnion.AgentWashington.Core;

public class Option
{
    public string Name { get; }
    public bool IsRequired { get; }
    public string Description { get; }
    public OptionType Type { get; }

    public Option(string name, OptionType type, string description = default, bool isRequired = default)
    {
        Name = name;
        Description = description;
        Type = type;
        IsRequired = isRequired;
    }
}