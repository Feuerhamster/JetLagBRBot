using LiteDB;

namespace JetLagBRBot.Models;

public enum EGameStatus
{
    NotStarted,
    ProtectionPhase,
    Running,
    Paused,
    Finished
}

public class Game<GameState, TeamGameState, PlayerGameState>(string name, DateTime startTime, int tgOperatorId)
{
    [BsonId] public Guid Id { get; set; } = new Guid();

    public string Name { get; set; } = name;
    
    public EGameStatus Status { get; set; } = EGameStatus.NotStarted;
    
    public DateTime StartTime { get; set; } = startTime;

    public int TelegramOperatorId { get; set; } = tgOperatorId;

    public GameState GameStateData { get; set; }
    
    public List<Team<TeamGameState, PlayerGameState>> Teams { get; set; } = [];
    
    public List<Player<PlayerGameState>> SoloPlayers { get; set; } = [];

    public Player<PlayerGameState>? GetPlayer(Guid playerId)
    {
        var player = this.SoloPlayers.FirstOrDefault(p => p.Id == playerId);

        if (player != null)
        {
            return player;
        }

        foreach (var team in this.Teams)
        {
            var p = team.Players.FirstOrDefault(p => p.Id == playerId);

            if (p != null)
            {
                return p;
            }
        }

        return null;
    }
}

public class Team<TeamGameState, PlayerGameState>(string name)
{
    public Guid Id { get; set; } = new Guid();

    public string Name { get; set; } = name;
    
    public List<Player<PlayerGameState>> Players { get; set; } = [];
    
    public TeamGameState TeamGameStateData { get; set; }
}

public class Player<PlayerGameState>(string nickname, int tgId)
{
    [BsonId]
    public Guid Id { get; set; } = new Guid();

    public int TelegramId { get; set; } = tgId;

    public string Nickname { get; set; } = nickname;
    
    public PlayerGameState PlayerGameStateData { get; set; }
}