namespace LBPUnion.AgentWashington.Core;

public sealed class UpdateInterval
{
    internal UpdateInterval(TimeSpan totalTime, TimeSpan deltaTime)
    {
        this.TotalTime = totalTime;
        this.DeltaTime = deltaTime;
    }
    
    public TimeSpan TotalTime { get; }
    public TimeSpan DeltaTime { get; }
}