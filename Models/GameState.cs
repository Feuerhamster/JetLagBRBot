using System.Text.Json.Serialization;
using JetLagBRBot.Utils;
using LiteDB;
using Telegram.Bot.Types;

namespace JetLagBRBot.Models;

public enum EGameStatus
{
    NotStarted,
    ProtectionPhase,
    Running,
    Stopped,
    Finished
}

public class Game<TGameState> where TGameState : class, new()
{
    [JsonConstructor]
    private Game()
    {
    }

    public Game(string name, long telegramGroupId)
    {
        this.Name = name;
        this.TelegramGroupId = telegramGroupId;
    }
    
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }
    
    public EGameStatus Status { get; set; } = EGameStatus.NotStarted;

    public long TelegramGroupId { get; set; }

    public TGameState GameStateData { get; set; } = new();
}

public class Team<TTeamGameState>(string name)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = name;
    
    public List<Guid> Players { get; set; } = [];
    
    public Location? Location { get; set; }
    
    [JsonInclude]
    public TTeamGameState TeamGameStateData { get; set; }
}

/// <summary>
/// Player class for a game
/// </summary>
/// <param name="nickname">Name that gets displayed publicly throughout the game</param>
/// <param name="tgId">Telegram user id of the player</param>
/// <typeparam name="TPlayerGameState">Gamemode specific data</typeparam>
public class Player<TPlayerGameState> where TPlayerGameState : class, new()
{
    [JsonConstructor]
    private Player()
    {
    }
    
    public Player(string nickname, long tgId)
    {
        this.Nickname = nickname;
        this.TelegramId = tgId;
    }
    
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();

    public long TelegramId { get; set; }

    public string Nickname { get; set; }
    
    public Location? Location { get; set; }

    [JsonIgnore]
    public string TelegramMention => TgFormatting.UserMention(TelegramId, Nickname);
    
    public TPlayerGameState PlayerGameStateData { get; set; } = new();
}

public class TempGameStateFile<TGameState, TPlayerGameState>() where TPlayerGameState : class, new() where TGameState : class, new()
{
    public Game<TGameState> Game { get; set; }
    public List<Player<TPlayerGameState>> Players { get; set; }
    public ManagedTimer GameTimer { get; set; }
}