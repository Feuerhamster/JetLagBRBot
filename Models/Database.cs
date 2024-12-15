using LiteDB;

namespace JetLagBRBot.Models;

public class DatabaseGame
{
    [BsonId]
    public Guid Id { get; set; }
    
    public string GameStateSerialized { get; set; }
}

public class DatabaseGameLog
{
    [BsonId]
    public Guid Id { get; set; }
    
    public Guid GameId { get; set; }
    
    public string Message { get; set; }
}

public class ImageCache
{
    public string FileName { get; set; }

    public string TgFileId { get; set; }
}