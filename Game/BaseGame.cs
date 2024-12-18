using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace JetLagBRBot.Game;

public interface IBaseGame
{
    public GameTemplate GameTemplate { get; }
    public bool PlayerJoin(long telegramId, string nickname);
    public bool LeavePlayer(long telegramId);
    public void StopGame();
    public void ResetGame();
    public void StartGame();
    public bool HasPlayer(long telegramId);
}

public class BaseGame<TGameState, TTeamGameState, TPlayerGameState> : IBaseGame where TGameState : class, new() where TPlayerGameState : class, new()
{
    private readonly ITelegramBotService _telegramBot; 
    
    public Game<TGameState> Game { get; private set; }

    public ManagedTimer GameTimer { get; }
    
    public GameTemplate GameTemplate { get; }

    // protected List<Team<TeamGameState>> Teams { get; private set; } = new();

    public List<Player<TPlayerGameState>> Players { get; private set; } = new();
    
    protected BaseGame(GameTemplate template, long telegramGroupId, IServiceProvider serviceProvider)
    {
        this._telegramBot = serviceProvider.GetService<ITelegramBotService>();
        this.Game = new Game<TGameState>(template.Config.Name, telegramGroupId);
        this.GameTemplate = template;
        
        this.GameTimer = new ManagedTimer(template.Config.Duration);
        this.GameTimer.OnTick += this.Tick;
        this.GameTimer.OnFinished += this.GameEnd;

        this._telegramBot.OnUserLocation += OnUserLocation;
    }

    private void Tick(object sender, EventArgs e)
    {
        this.CheckProtectionTime();
    }

    private async Task CheckProtectionTime()
    {
        if (this.Game.Status != EGameStatus.ProtectionPhase) return;
        if (this.GameTemplate.Config.ProtectionPhase is not TimeSpan protectionPhase) return;
        
        if (DateTime.Now >= this.GameTimer.TimeStarted.Add(protectionPhase))
        {
            await this.BroadcastMessage("\u2622\ufe0f Protection phase is over!");
            this.Game.Status = EGameStatus.Running;
        }
    }

    private void OnUserLocation(object? sender, Message msg)
    {
        var p = this.Players.FirstOrDefault(p => p.TelegramId == msg.From.Id);

        if (p == null) return;

        p.Location = msg.Location;
    }

    private void GameEnd(object sender, EventArgs e)
    {
        this.Game.Status = EGameStatus.Finished;
        this.BroadcastMessage("\ud83c\udfc1 The game has finished!");
    }

    public bool HasPlayer(long telegramId)
    {
        return this.Players.Exists(p => p.TelegramId == telegramId);
    }
    
    /// <summary>
    /// Add a player to this game
    /// </summary>
    /// <param name="telegramId">Telegram User ID</param>
    /// <param name="nickname"></param>
    public bool PlayerJoin(long telegramId, string nickname)
    {
        if (this.Players.Exists(p => p.TelegramId == telegramId)) return false;
        
        Player<TPlayerGameState> p = new(nickname, telegramId);
        this.Players.Add(p);

        this.BroadcastMessage($"{p.Nickname} joined the game");
        
        return true;
    }

    /// <summary>
    /// Remove a player from the game
    /// </summary>
    /// <param name="telegramId">Telegram User ID</param>
    /// <returns>true if player left, false if player wasn't there in the first place</returns>
    public bool LeavePlayer(long telegramId)
    {
        var p = this.Players.FirstOrDefault(p => p.TelegramId == telegramId);

        if (p == null) return false;
        
        this.Players.Remove(p);
        
        this.BroadcastMessage($"{p.Nickname} left the game");
        
        return true;
    }

    /// <summary>
    /// Start the game
    /// </summary>
    public void StartGame()
    {
        // TODO: make async
        this.GameTimer.Start();
        
        this.BroadcastMessage("\ud83d\udfe2 The game has been started!");

        if (this.GameTemplate.Config.ProtectionPhase != null)
        {
            this.Game.Status = EGameStatus.ProtectionPhase;
            
            var protectionUntil = DateTime.Now.Add((TimeSpan)this.GameTemplate.Config.ProtectionPhase).ToString("HH:mm:ss");

            this.BroadcastMessage($"\ud83d\udee1\ufe0f Protection phase unitl {protectionUntil}");
        }
        else
        {
            this.Game.Status = EGameStatus.Running;
        }
    }

    /// <summary>
    /// Stop the game. This is can also be used to pause the game because this doesn't end it.
    /// </summary>
    public void StopGame()
    {
        this.GameTimer.Stop();
        this.Game.Status = EGameStatus.Stopped;
        this.BroadcastMessage("\ud83d\udd34 The game has been stopped!");
    }

    public void ResetGame()
    {
        this.GameTimer.Reset();
        this.Game = new Game<TGameState>(GameTemplate.Config.Name, this.Game.TelegramGroupId);
        this.Players.Clear();
        this.BroadcastMessage("ℹ\ufe0f The game has been reset!");
    }

    /// <summary>
    /// Sends a message into the telegram group where the game takes place
    /// </summary>
    /// <param name="message">text message for the group</param>
    public async Task BroadcastMessage(string message, IReplyMarkup replyMarkup = null)
    {
        message = TgFormatting.MarkdownEscape(message);
        await this._telegramBot.Client.SendMessage(this.Game.TelegramGroupId, message, replyMarkup: replyMarkup, parseMode: ParseMode.MarkdownV2);
    }
    
    /// <summary>
    /// Sends a message to the player
    /// </summary>
    /// <param name="message">text message for the player</param>
    public async Task SendPlayerMessage(Guid playerId, string message, IReplyMarkup replyMarkup = null)
    {
        var p = this.GetPlayerById(playerId);
        
        message = TgFormatting.MarkdownEscape(message);
        await this._telegramBot.Client.SendMessage(p.TelegramId, message, replyMarkup: replyMarkup, parseMode: ParseMode.MarkdownV2);
    }

    public Player<TPlayerGameState>? GetPlayerById(Guid playerId)
    {
        return this.Players.FirstOrDefault(p => p.Id.Equals(playerId));
    }
}