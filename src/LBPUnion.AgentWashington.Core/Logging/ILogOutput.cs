namespace LBPUnion.AgentWashington.Core.Logging;

public interface ILogOutput
{
    void AppendLog(LogLevel level, DateTime time, string callerPath, int lineNumber, string callerMemberName,
        Type callerClass, string message);
}