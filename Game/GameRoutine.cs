using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot.Types;

namespace JetLagBRBot.Game;

public abstract class GameRoutine<GameState, TeamState, PlayerState>
{
    protected Game<GameState, TeamState, PlayerState> State { get; private set; }
    private Dictionary<Guid, Location> PlayerLocations { get; set; }
    private readonly ITelegramBotService _telegramBot;

    public GameRoutine(ITelegramBotService tg)
    {
        this._telegramBot = tg;
    }

    /// <summary>
    /// Create a new game state instance
    /// </summary>
    /// <param name="name">Name of this game</param>
    /// <param name="startTime">When does the game takes place</param>
    /// <param name="tgOperatorId">telegram id of the operator user</param>
    protected void CreateGame(string name, DateTime startTime, int tgOperatorId)
    {
        this.State = new Game<GameState, TeamState, PlayerState>(name, startTime, tgOperatorId);
    }

    protected Location GetPlayerLocation(Guid playerId)
    {
        return this.PlayerLocations[playerId];
    }
}