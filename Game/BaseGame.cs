using JetLagBRBot.Models;
using JetLagBRBot.Services;

namespace JetLagBRBot.Game;

public interface IBaseGame
{
}

public abstract class BaseGame<GameState, TeamGameState, PlayerGameState> : IBaseGame
{
    private readonly ITelegramBotService _telegramBot; 
    
    protected Game<GameState> Game { get; private set; }

    // protected List<Team<TeamGameState>> Teams { get; private set; } = new();

    protected List<Player<PlayerGameState>> Players { get; private set; } = new();
    
    protected BaseGame(GameTemplate template, IServiceProvider serviceProvider)
    {
        this._telegramBot = serviceProvider.GetService<ITelegramBotService>();
        this.Game = new Game<GameState>();
    }

    /// <summary>
    /// Add a player to this game
    /// </summary>
    /// <param name="telegramId"></param>
    /// <param name="nickname"></param>
    public void PlayerJoin(int telegramId, string nickname)
    {
        Player<PlayerGameState> p = new(nickname, telegramId);
        this.Players.Add(p);
    }

    public bool LeavePlayer(int telegramId)
    {
        var p = this.Players.FirstOrDefault(p => p.TelegramId == telegramId);

        if (p == null) return false;
        
        this.Players.Remove(p);
        return true;
    }
}