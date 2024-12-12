using System.Text;
using JetLagBRBot.Game;
using JetLagBRBot.Game.Modes.BattleRoyale.Commands;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JetLagBRBot.Game.Modes.BattleRoyale;

public class BattleRoyaleGamemode : BaseGame<GameStateData, PlayerOrTeamStateData, PlayerOrTeamStateData>
{
    public const string GameModeName = "BattleRoyale";

    private readonly TimeSpan TimeBetweenDrops;
    
    private readonly ITelegramBotService _telegramBot;
    
    private readonly IServiceProvider _services;

    public event EventHandler<TagEventArgs> OnPlayerTag = delegate { }; 
    
    public event EventHandler<SuccessfulTagEventArgs> OnSuccessfulPlayerTag = delegate { };
    
    public event EventHandler<PowerUpUseEventArgs> OnPowerUpUse = delegate { };
    
    public event EventHandler<Guid> OnSuccessfulPowerUpUse = delegate { };
    
    public BattleRoyaleGamemode(GameTemplate template, BattleRoyaleGameData data, long telegramGroupId, IServiceProvider services) : base(template, telegramGroupId, services)
    {
        this.TimeBetweenDrops = data.TimeBetweenDrops;
        this._services = services;
        this.Game.GameStateData.Landmarks = new(data.Landmarks);
        
        var commandService = services.GetRequiredService<ICommandService>();
        this._telegramBot = services.GetRequiredService<ITelegramBotService>();
        
        commandService.AddCommand<TagCommand>();

        this.GameTimer.OnTick += this.Tick;
        this.GameTimer.OnFinished += this.TimerFinised;

        this.OnSuccessfulPlayerTag += this.CheckIfPlayerDead;
    }
    
    private void Tick(object? sender, EventArgs e)
    {
        this.CheckForNextLandmark();
    }

    private void TimerFinised(object? sender, EventArgs e)
    {
        this.FinishGame();
    }

    private void CheckForNextLandmark()
    {
        if (this.Game.Status != EGameStatus.Running) return;

        if (DateTime.Now < this.Game.GameStateData.LastTimeDropped.Add(this.TimeBetweenDrops)) return;

        this.NewLandmark();
    }

    /// <summary>
    /// Select a new landmark (this overwrites the current one)
    /// </summary>
    private async Task NewLandmark()
    {
        var rand = new Random();
        
        // select new drop
        var newLandmark = this.Game.GameStateData.Landmarks
            .Where(l => l.District != this.Game.GameStateData.CurrentActiveLandmark.District)
            .OrderBy(l => rand.Next())
            .FirstOrDefault();

        if (newLandmark == null) return;
        
        this.Game.GameStateData.CurrentActiveLandmark = newLandmark;
        this.Game.GameStateData.Landmarks.Remove(newLandmark);

        StringBuilder message = new StringBuilder();
        message.AppendLine("\ud83d\udccc **New Powerup available at Landmark**");
        message.AppendLine("");
        message.AppendLine($"Name: **{newLandmark.Name}**");
        message.AppendLine($"District: **{newLandmark.District}**");

        string locationLink = $"https://www.google.com/maps/place/{newLandmark.Coordinates[0]},{newLandmark.Coordinates[1]}";
        
        message.AppendLine($"Location: **[{newLandmark.Coordinates[0]}, {newLandmark.Coordinates[1]}]({locationLink})**");

        await this.BroadcastMessage(message.ToString());
        
        var fileStream = System.IO.File.OpenRead(newLandmark.Image);
        await this._telegramBot.Client.SendPhoto(this.Game.TelegramGroupId, new InputFileStream(fileStream));
    }

    /// <summary>
    /// Execute the routine for tagging a player
    /// </summary>
    /// <param name="taggerId">Player Id of the tagger</param>
    /// <param name="victimId">Player Id of the victim</param>
    public async Task TagPlayer(Guid taggerId, Guid victimId)
    {
        if (this.Game.Status != EGameStatus.Running) return;
        
        var victim = this.GetPlayerById(victimId);
        var tagger = this.GetPlayerById(taggerId);
        
        if (victim == null || tagger == null) return;

        if (victim.PlayerGameStateData.HealthPoints == 0)
        {
            this.SendPlayerMessage(tagger.Id, "\u26a0\ufe0f Tag failed. Targeted player already dead.");
            return;
        }
        
        var tag = new PlayerTag(tagger.Id, victim.Id);
        
        // fire event
        var eventArgs = new TagEventArgs(tag);
        this.OnPlayerTag.Invoke(this, eventArgs);
        
        if (eventArgs.Cancel)
        {
            await this.BroadcastMessage(
                $"\u274e The tag of {tagger.TelegramMention} on {victim.TelegramMention} was canceled due to a power up of them!");
            return;
        }
        
        this.OnSuccessfulPlayerTag.Invoke(this, new SuccessfulTagEventArgs(tag));
        
        this.Game.GameStateData.PlayerTags.Add(tag);

        victim.PlayerGameStateData.HealthPoints -= tag.Damage;
        
        await this.BroadcastMessage($"\ud83d\udea9 Player {tagger.TelegramMention} successfully tagged {victim.TelegramMention}!");
    }

    /// <summary>
    /// Claim a landmark
    /// </summary>
    /// <param name="claimerId">Player id of the player who claims the landmark</param>
    /// <returns>true if successful, false if failed</returns>
    public async Task<bool> ClaimLandmark(Guid claimerId)
    {
        if (this.Game.Status != EGameStatus.Running) return false;
        var lm = this.Game.GameStateData.CurrentActiveLandmark;
        
        if (lm == null || lm.Claimed)
        {
            return false;
        }

        var claimer = this.GetPlayerById(claimerId);

        if (claimer == null) return false;
        
        // TODO: implement actual powerup claim
        var randomPowerUp = PowerUpUtils.RandomPowerUp();
        var powerUp = PowerUpUtils.GetPowerUpInstance(randomPowerUp, this, claimer.Id);
        
        claimer.PlayerGameStateData.Powerups.Add(powerUp);
        
        await this.BroadcastMessage($"\ud83c\udf1f {claimer.TelegramMention} claimed the PowerUp at the current Landmark \"{lm.Name}\"");
        return true;
    }

    /// <summary>
    /// Activate a PowerUp for a Player. If the player already has one active, the active one expires immediately in favor of the new one
    /// </summary>
    /// <param name="playerId">Id of the player</param>
    /// <param name="powerUpId">Id of the power up</param>
    /// <returns>true if usage was successful, false if it was not used successfully</returns>
    public async Task<bool> UsePowerUp(Guid playerId, Guid powerUpId)
    {
        if (this.Game.Status != EGameStatus.Running) return false;
        
        var player = this.GetPlayerById(playerId);

        if (player == null) return false;
        
        // check if player has already active powerup
        var activePowerUps = player.PlayerGameStateData.Powerups.Where(p => p.Status == EPowerUpStatus.Active).ToList();

        // expire the active power up because the player can have only one power up usage at a time
        foreach (var activePowerUp in activePowerUps)
        {
            activePowerUp.Expire();
        }
        
        var powerUp = player.PlayerGameStateData.Powerups.FirstOrDefault(p => p.Id.Equals(powerUpId));
        
        if (powerUp == null) return false;

        var eventArgs = new PowerUpUseEventArgs(powerUp, playerId);
        
        OnPowerUpUse.Invoke(this, eventArgs);

        if (eventArgs.Cancel) return false;
        
        powerUp.Use();
        
        OnSuccessfulPowerUpUse.Invoke(this, powerUp.Id);
        
        return true;
    }

    public void CheckIfPlayerDead(object? sender, SuccessfulTagEventArgs e)
    {
        var victim = this.GetPlayerById(e.PlayerTag.VictimId);
        
        if (victim.PlayerGameStateData.HealthPoints == 0)
        {
            this.BroadcastMessage($"\ud83d\udc80 The player {victim.TelegramMention} is now dead");
        }
        
        var aliveCount = this.Players.Count(p => p.PlayerGameStateData.HealthPoints > 0);

        if (aliveCount <= 1)
        {
            this.FinishGame();
        }
    }

    public async Task FinishGame()
    {
        this.Game.Status = EGameStatus.Finished;
        
        var winner = this.CalculateWinner();
        
        var player = this.GetPlayerById(winner);
        
        await this.BroadcastMessage($"\ud83c\udfc6 The player {player.TelegramMention} has won the game!");
        
        this.PrintScoreboard();
    }
    
    /// <summary>
    /// Calculate winner by this formula: Tags Count > HP > PowerUp Count
    /// </summary>
    /// <returns>Id of the player who won</returns>
    public Guid CalculateWinner()
    {
        // If only one player is alive, it automatically won
        var playersAlive = this.Players.Where(p => p.PlayerGameStateData.HealthPoints > 0).ToArray();
        if (playersAlive.Length == 1)
        {
            return playersAlive[0].Id;
        }

        var playerTagsCount = new Dictionary<Guid, int>();
        
        foreach (var player in playersAlive)
        {
            var tags = this.Game.GameStateData.PlayerTags.Count(t => t.TaggerId.Equals(player.Id));
            playerTagsCount.Add(player.Id, tags);
        }
        
        int maxTags = playerTagsCount.Values.DefaultIfEmpty(0).Max();
        
        var tagCandidates = playersAlive.Where(p => playerTagsCount.ContainsKey(p.Id) && playerTagsCount[p.Id] == maxTags).ToList();

        if (tagCandidates.Count == 1)
        {
            return tagCandidates.First().Id;
        }
        
        // Wenn es mehrere Kandidaten gibt, prüfe nach HP
        int maxHP = tagCandidates.Max(p => p.PlayerGameStateData.HealthPoints);
        var hpCandidates = tagCandidates.Where(p => p.PlayerGameStateData.HealthPoints == maxHP).ToList();

        if (hpCandidates.Count == 1)
        {
            // Wenn es nur einen Spieler mit den meisten HP gibt, ist dieser der Gewinner
            return hpCandidates.First().Id;
        }

        // Wenn es mehrere Kandidaten gibt, zähle die PowerUps
        return hpCandidates.OrderByDescending(p => p.PlayerGameStateData.Powerups.Count).First().Id;
    }

    public void PrintScoreboard()
    {
        var text = new StringBuilder();

        text.AppendLine("**Final player stats:**\n");

        foreach (var player in this.Players)
        {
            text.AppendLine($"**{player.TelegramMention}:**");

            text.AppendLine($"\ud83d\udc9a Health points: **{player.PlayerGameStateData.HealthPoints}**");
            text.AppendLine($"\ud83c\udf1f Power ups: **{player.PlayerGameStateData.Powerups.Count}**");
            
            var tags = this.Game.GameStateData.PlayerTags.Where(t => t.TaggerId.Equals(player.Id));
            text.AppendLine($"\ud83d\udea9 Tags: **{tags}**");
            text.AppendLine("");
        }
        
        this.BroadcastMessage(text.ToString());
    }
}