namespace LBPUnion.AgentWashington.Core.Settings;

public abstract class AsyncConfigurable : IConfigurable
{
    public string GetValue(ConfigurableContext ctx)
    {
        var awaiter = GetValueAsync(ctx).GetAwaiter();
        return awaiter.GetResult();
    }

    public void SetValue(ConfigurableContext ctx, string value)
    {
        SetValueAsync(ctx, value).Wait();
    }

    protected abstract Task<string> GetValueAsync(ConfigurableContext ctx);
    protected abstract Task SetValueAsync(ConfigurableContext ctx, string value);
}