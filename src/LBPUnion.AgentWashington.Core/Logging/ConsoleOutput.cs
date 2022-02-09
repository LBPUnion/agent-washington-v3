namespace LBPUnion.AgentWashington.Core.Logging;

public class ConsoleOutput : ILogOutput
{
    public void AppendLog(LogLevel level, DateTime time, string callerPath, int lineNumber, string callerMemberName,
        Type callerClass, string message)
    {
        var color = level switch
        {
            LogLevel.Info => ConsoleColor.Gray,
            LogLevel.Message => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.DarkYellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            LogLevel.Debug => ConsoleColor.DarkGray,
            _ => ConsoleColor.Gray
        };

        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        Console.WriteLine($"[{time}] <{callerClass.Name}::{callerMemberName}()/{level}> {message}");

        Console.ForegroundColor = oldColor;
    }
}