using System.Diagnostics;

namespace LBPUnion.AgentWashington.Core.Logging;

public sealed class DebugLog : ILogOutput
{
    public void AppendLog(LogLevel level, DateTime time, string callerPath, int lineNumber, string callerMemberName,
        Type callerClass, string message)
    {
        Debug.WriteLine(
            $"[{time}] {callerPath}:{lineNumber} | {callerClass.FullName}::{callerMemberName}() | <{level}> {message}");
    }
}