using JetLagBRBot.Models;
using JetLagBRBot.Services;

namespace JetLagBRBot.Game;

public abstract class BaseGame<GameState, TeamGameState, PlayerGameState>
{
    private readonly ITelegramBotService _telegramBot; 
    
    protected Game<GameState> Game { get; private set; }

    protected List<Team<TeamGameState>> Teams { get; private set; } = new();

    protected List<Player<PlayerGameState>> Players { get; private set; } = new();
    
    protected BaseGame(IServiceProvider serviceProvider)
    {
        this._telegramBot = serviceProvider.GetService<ITelegramBotService>();
    }

    /// <summary>
    /// Initializes a new game or overwrites the existing one
    /// </summary>
    /// <param name="name">Name of the game</param>
    /// <param name="startTime">When does the game takes place?</param>
    /// <param name="tgOperatorId">Telegram Id of the operational user</param>
    /// <returns>Id of the new game</returns>
    public Guid InitGame(string name, int telegramGroupId)
    {
        this.Game = new Game<GameState>(name, telegramGroupId);
        
        return this.Game.Id;
    }
}