using LiteDB;

namespace LBPUnion.AgentWashington.Core.Persistence;

public class Database
{
    private LiteDatabase _db;
    
    internal Database(LiteDatabase database)
    {
        _db = database;
    }

    public ILiteCollection<T> GetCollection<T>(string collectionName) where T : IDatabaseObject
    {
        return _db.GetCollection<T>(collectionName);
    }
}