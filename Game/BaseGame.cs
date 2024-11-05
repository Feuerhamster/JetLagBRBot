using JetLagBRBot.Models;
using JetLagBRBot.Services;

namespace JetLagBRBot.Game;

public abstract class BaseGame<GameState, TeamGameState, PlayerGameState>
{
    private readonly ITelegramBotService _telegramBot; 
    
    protected Game<GameState, TeamGameState, PlayerGameState> _gameState { get; private set; }
    
    public BaseGame(IServiceProvider serviceProvider)
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
    public Guid InitGame(string name, DateTime startTime, int tgOperatorId)
    {
        this._gameState = new Game<GameState, TeamGameState, PlayerGameState>(name, startTime, tgOperatorId);
        
        return this._gameState.Id;
    }

    public Guid AddTeam(string name)
    {
        Team<TeamGameState, PlayerGameState> team = new(name);

        this._gameState.Teams.Add(team);
        
        return team.Id;
    }

    public void DeleteTeam(Guid id)
    {
        var team = this._gameState.Teams.Find(id => id.Equals(id));
        
        this._gameState.Teams.Remove(team);
    }

    /// <summary>
    /// Add player to the game and a team or as solo
    /// </summary>
    /// <param name="teamId">Id of the team or null if solo</param>
    /// <param name="nickname">Nickname of the player</param>
    /// <param name="tgId">Telegram id of the player</param>
    /// <returns>Id of the new player of null if failed</returns>
    public Guid? AddPlayerToTeam(Guid? teamId, string nickname, int tgId)
    {
        Player<PlayerGameState> player = new(nickname, tgId);

        if (teamId == null)
        {
            this._gameState.SoloPlayers.Add(player);
            return player.Id;
        }
        
        var team = this._gameState.Teams.Find(t => t.Id == teamId);
        if (team == null)
        {
            return null;
        }

        team.Players.Add(player);
        
        return player.Id;
    }

    /// <summary>
    /// Remove a player from a team or the game
    /// </summary>
    /// <param name="teamId">Id of the team or null if solo</param>
    /// <param name="playerId">Id of the player</param>
    public void RemovePlayerFromTeam(Guid? teamId, Guid playerId)
    {
        if (teamId == null)
        {
            var p = this._gameState.SoloPlayers.Find(p => p.Id.Equals(playerId));

            if (p == null)
            {
                return;
            }
            
            this._gameState.SoloPlayers.Remove(p);
            return;
        }
        
        var team = this._gameState.Teams.Find(t => t.Id == teamId);

        if (team == null) return;

        var player = team.Players.Find(p => p.Id.Equals(playerId));
        
        team.Players.Remove(player);
    }
}