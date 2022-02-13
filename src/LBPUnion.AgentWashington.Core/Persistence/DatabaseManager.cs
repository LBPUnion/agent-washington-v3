using LBPUnion.AgentWashington.Core.Logging;
using LiteDB;

namespace LBPUnion.AgentWashington.Core.Persistence;

public class DatabaseManager : BotModule
{
    private string _filePath;
    private object _syncLock = new object();

    protected override void BeforeInit()
    {
        var dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _filePath = Path.Combine(dataDirectory, "awdatabase.db");

        Logger.Log($"Bot database is stored in {_filePath}");
    }

    public void OpenDatabase(Action<Database> action)
    {
        lock (_syncLock)
        {
            using var db = new LiteDatabase(_filePath);

            action(new Database(db));
            
            // Yes. I'm insane.
            db.Dispose();
        }
    }
}