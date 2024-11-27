using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Timer = System.Timers.Timer;

namespace JetLagBRBot.Game;

public interface IBaseGame
{
    public GameTemplate GameTemplate { get; }
}

public abstract class BaseGame<GameState, TeamGameState, PlayerGameState> : IBaseGame
{
    private readonly ITelegramBotService _telegramBot; 
    
    public Game<GameState> Game { get; private set; }

    private ManagedTimer GameTimer;
    
    public GameTemplate GameTemplate { get; }

    // protected List<Team<TeamGameState>> Teams { get; private set; } = new();

    protected List<Player<PlayerGameState>> Players { get; private set; } = new();
    
    public event EventHandler OnGameStart;
    public event EventHandler OnGameStop;
    
    protected BaseGame(GameTemplate template, long telegramGroupId, IServiceProvider serviceProvider)
    {
        this._telegramBot = serviceProvider.GetService<ITelegramBotService>();
        this.Game = new Game<GameState>(template.Config.Name, telegramGroupId);
        this.GameTemplate = template;
        this.GameTimer = new ManagedTimer(template.Config.Duration);
    }

    /// <summary>
    /// Add a player to this game
    /// </summary>
    /// <param name="telegramId"></param>
    /// <param name="nickname"></param>
    public void PlayerJoin(long telegramId, string nickname)
    {
        Player<PlayerGameState> p = new(nickname, telegramId);
        this.Players.Add(p);
    }

    public bool LeavePlayer(long telegramId)
    {
        var p = this.Players.FirstOrDefault(p => p.TelegramId == telegramId);

        if (p == null) return false;
        
        this.Players.Remove(p);
        return true;
    }

    public void StartGame()
    {
        this.GameTimer.Start();
        this.OnGameStart.Invoke(this, EventArgs.Empty);
        this.BroadcastMessage("\ud83d\udfe2 The game has been started!");
    }

    public void StopGame()
    {
        this.GameTimer.Stop();
        this.OnGameStop.Invoke(this, EventArgs.Empty);
        this.BroadcastMessage("\ud83d\udd34 The game has been stopped!");
    }

    public void ResetGame()
    {
        this.GameTimer.Reset();
    }

    /// <summary>
    /// Sends a message into the telegram group where the game takes place
    /// </summary>
    /// <param name="message">text message for the group</param>
    public async Task BroadcastMessage(string message, IReplyMarkup replyMarkup = null)
    {
        this._telegramBot.Client.SendMessage(this.Game.TelegramGroupId, message, replyMarkup: replyMarkup);
    }
}