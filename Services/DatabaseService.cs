using JetLagBRBot.Models;
using LiteDB;

namespace JetLagBRBot.Services;

public interface IDatabaseService
{
    
}

public class DatabaseService : IDatabaseService
{
    private readonly LiteDatabase db;
    
    DatabaseService(IConfiguration config)
    {
        string dbPath = config.GetValue<string>("DatabasePath");
        this.db = new LiteDatabase(dbPath);
    }
}