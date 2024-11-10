using LiteDB;
using Telegram.Bot.Types;

namespace JetLagBRBot.Models;

public enum EGameStatus
{
    NotStarted,
    ProtectionPhase,
    Running,
    Paused,
    Finished
}

public class Game<GameState>(string name, long telegramGroupId)
{
    [BsonId] public Guid Id { get; set; } = new Guid();

    public string Name { get; set; } = name;
    
    public EGameStatus Status { get; set; } = EGameStatus.NotStarted;

    public long TelegramGroupId { get; set; } = telegramGroupId;

    public GameState GameStateData { get; set; }
}

public class Team<TeamGameState>(string name)
{
    public Guid Id { get; set; } = new Guid();

    public string Name { get; set; } = name;
    
    public List<Guid> Players { get; set; } = [];
    
    public Location? Location { get; set; }
    
    public TeamGameState TeamGameStateData { get; set; }
}

public class Player<PlayerGameState>(string nickname, int tgId)
{
    [BsonId]
    public Guid Id { get; set; } = new Guid();

    public int TelegramId { get; set; } = tgId;

    public string Nickname { get; set; } = nickname;
    
    public Location? Location { get; set; }
    
    public PlayerGameState PlayerGameStateData { get; set; }
}