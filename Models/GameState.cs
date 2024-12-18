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

public class Game<TGameState>(string name, long telegramGroupId) where TGameState : class, new()
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = name;
    
    public EGameStatus Status { get; set; } = EGameStatus.NotStarted;

    public long TelegramGroupId { get; set; } = telegramGroupId;

    public TGameState GameStateData { get; set; } = new();
}

public class Team<TTeamGameState>(string name)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = name;
    
    public List<Guid> Players { get; set; } = [];
    
    public Location? Location { get; set; }
    
    public TTeamGameState TeamGameStateData { get; set; }
}

/// <summary>
/// Player class for a game
/// </summary>
/// <param name="nickname">Name that gets displayed publicly throughout the game</param>
/// <param name="tgId">Telegram user id of the player</param>
/// <typeparam name="TPlayerGameState">Gamemode specific data</typeparam>
public class Player<TPlayerGameState>(string nickname, long tgId) where TPlayerGameState : class, new()
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();

    public long TelegramId { get; set; } = tgId;

    public string Nickname { get; set; } = nickname;
    
    public Location? Location { get; set; }

    public string TelegramMention => TgFormatting.UserMention(TelegramId, Nickname);

    public TPlayerGameState PlayerGameStateData { get; set; } = new();
}