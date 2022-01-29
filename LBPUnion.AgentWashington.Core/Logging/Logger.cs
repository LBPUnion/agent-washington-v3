using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LBPUnion.AgentWashington.Core.Logging;

public static class Logger
{
    private static List<ILogOutput> _outputs = new List<ILogOutput>();

    static Logger()
    {
        RegisterOutput(new DebugLog());
        RegisterOutput(new ConsoleOutput());
    }
    
    internal static void RegisterOutput(ILogOutput output)
    {
        if (output == null)
            throw new ArgumentNullException(nameof(output));

        _outputs.Add(output);
    }

    public static void Log(string message, LogLevel level = LogLevel.Info, [CallerMemberName] string member = "",
        [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = null)
    {
        var stackFrame = new StackFrame(1);
        var caller = stackFrame.GetMethod();

        var callingClass = caller.DeclaringType;

        var date = DateTime.Now;

        foreach (var output in _outputs)
        {
            output.AppendLog(level, date, filePath, lineNumber, member, callingClass, message);
        }
    }
}