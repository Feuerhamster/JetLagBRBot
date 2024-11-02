using LiteDB;

namespace JetLagBRBot.Models;

public class Game<GameState, TeamGameState, PlayerGameState>(string name, DateTime startTime, int tgOperatorId)
{
    [BsonId] public Guid Id { get; set; } = new Guid();

    public string Name { get; set; } = name;
    
    public DateTime StartTime { get; set; } = startTime;

    public int TelegramOperatorId { get; set; } = tgOperatorId;

    public GameState GameStateData { get; set; }
    
    public List<Team<TeamGameState, PlayerGameState>> Teams { get; set; } = [];
    
    public List<Player<PlayerGameState>> SoloPlayers { get; set; } = [];
}

public class Team<TeamGameState, PlayerGameState>(string name)
{
    public Guid Id { get; set; } = new Guid();

    public string Name { get; set; } = name;
    
    public List<Player<PlayerGameState>> Players { get; set; } = [];
    
    public TeamGameState TeamGameStateData { get; set; }
}

public class Player<PlayerGameState>(string nickname)
{
    [BsonId]
    public Guid Id { get; set; } = new Guid();
    
    public int TelegramId { get; set; }

    public string Nickname { get; set; } = nickname;
    
    public PlayerGameState PlayerGameStateData { get; set; }
}