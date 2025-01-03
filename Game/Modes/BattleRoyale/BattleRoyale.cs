using System.Globalization;
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
    
    
    private readonly ITelegramBotService _telegramBot;
    
    private readonly ILogger<BattleRoyaleGamemode> _logger;
    
    private readonly IDatabaseService _database;

    private readonly BattleRoyaleGameData GameData;
    
    private readonly WeightedAndBalancedRandomSelector<EPowerUp> _powerUpSelector;

    // TODO: make all event handlers async
    
    public event EventHandler<TagEventArgs> OnPlayerTag = delegate { }; 
    
    public event EventHandler<SuccessfulTagEventArgs> OnSuccessfulPlayerTag = delegate { };
    
    public event EventHandler<PowerUpUseEventArgs> OnPowerUpUse = delegate { };

    public event EventHandler<PowerUpUseEventArgs> OnLandmarkClaim = delegate { }; 
    
    public event EventHandler<Guid> OnSuccessfulPowerUpUse = delegate { };
    
    public BattleRoyaleGamemode(GameTemplate template, BattleRoyaleGameData data, long telegramGroupId, IServiceProvider services) : base(template, telegramGroupId, services)
    {
        this._database = services.GetRequiredService<IDatabaseService>();
        this._logger = services.GetRequiredService<ILogger<BattleRoyaleGamemode>>();
        
        this.Game.GameStateData.Landmarks = new(data.Landmarks);
        
        this.GameData = data;
        
        var commandService = services.GetRequiredService<ICommandService>();
        this._telegramBot = services.GetRequiredService<ITelegramBotService>();
        
        commandService.AddCommand<TagCommand>();
        commandService.AddCommand<ClaimCommand>();
        commandService.AddCommand<InvCommand>();
        commandService.AddCommand<UseCommand>();
        commandService.AddCommand<UndoTagCommand>();
        this._telegramBot.UpdateCommands();

        this._powerUpSelector = new(PowerUpUtils.WeightedPowerUps);
        
        this.GameTimer.OnTick += this.Tick;
        this.GameTimer.OnFinished += this.TimerFinised;

        this.OnSuccessfulPlayerTag += this.CheckIfPlayerDead;
    }
    
    private void Tick(object? sender, EventArgs e)
    {
        this.CheckForNextLandmark();
        this.PostTagProtectionAlgorithm();
    }

    private void TimerFinised(object? sender, EventArgs e)
    {
        this.FinishGame();
    }

    private void CheckForNextLandmark()
    {
        if (this.Game.Status != EGameStatus.Running) return;

        if (
            ManagedTimer.VerifyTimeIsOver(this.Game.GameStateData.LastTimeDropped, this.GameData.TimeBetweenDrops) ||
            this.Game.GameStateData.CurrentActiveLandmark == null
        ) {
            this.NewLandmark();
        }
    }

    private async Task PostTagProtectionAlgorithm()
    {
        var players = this.Players.Where(p => p.PlayerGameStateData.TagStatus != EPlayerTagStatus.Default);

        foreach (var player in players)
        {
            var tag = this.Game.GameStateData.PlayerTags
                .Where(t => t.VictimId.Equals(player.Id))
                .OrderByDescending(t => t.TagTime)
                .FirstOrDefault();

            if (tag == null)
            {
                continue;
            }

            // player is frozen but freeze is over
            if (player.PlayerGameStateData.TagStatus == EPlayerTagStatus.Frozen &&
                ManagedTimer.VerifyTimeIsOver(tag.TagTime, this.GameData.TagFreeze))
            {
                player.PlayerGameStateData.TagStatus = EPlayerTagStatus.Protected;
                await this.SendPlayerMessage(player.Id, $"\u2744\ufe0f You are not frozen anymore and are allowed to move again.");
                await this.BroadcastMessage($"\u2744\ufe0f {player.TelegramMention} is now allowed to move again", escape: false);
                continue;
            }
            
            // player is protected but personal protection after tag expired
            if (player.PlayerGameStateData.TagStatus == EPlayerTagStatus.Protected &&
                ManagedTimer.VerifyTimeIsOver(tag.TagTime, this.GameData.AfterTagProtection))
            {
                player.PlayerGameStateData.TagStatus = EPlayerTagStatus.Default;
                await this.SendPlayerMessage(player.Id, $"\ud83d\udee1\ufe0f You are not protected anymore and you can now be tagged again.");
                await this.BroadcastMessage($"\ud83d\udee1\ufe0f {player.TelegramMention} can now be tagged again", escape: false);
                continue;
            }
        }
    }

    /// <summary>
    /// Select a new landmark (this overwrites the current one)
    /// </summary>
    private async Task NewLandmark()
    {
        var rand = new Random();
        
        // select new drop
        var newLandmark = this.Game.GameStateData.Landmarks
            .Where(l => (this.Game.GameStateData.CurrentActiveLandmark == null || l.District != this.Game.GameStateData.CurrentActiveLandmark.District) || this.Game.GameStateData.Landmarks.All(c => c.District == l.District))
            .OrderBy(l => rand.Next())
            .FirstOrDefault();

        if (newLandmark == null)
        {
            this._logger.LogWarning("No new landmark was found!");
            return;
        }
        
        this.Game.GameStateData.CurrentActiveLandmark = newLandmark;
        this.Game.GameStateData.Landmarks.Remove(newLandmark);
        
        this.Game.GameStateData.LastTimeDropped = DateTime.Now;

        StringBuilder message = new StringBuilder();
        message.AppendLine("\ud83d\udccc *New Powerup available at Landmark*");
        message.AppendLine("");
        message.AppendLine($"Name: *{TgFormatting.MarkdownEscape(newLandmark.Name)}*");
        message.AppendLine($"District: *{TgFormatting.MarkdownEscape(newLandmark.District)}*");

        var lat = newLandmark.Coordinates[0].ToString(CultureInfo.InvariantCulture);
        var lng = newLandmark.Coordinates[1].ToString(CultureInfo.InvariantCulture);
        
        string locationLink = $"https://www.google.com/maps/place/{lat},{lng}";
        
        message.AppendLine($"Location: *[{TgFormatting.MarkdownEscape(newLandmark.Coordinates[0].ToString())}, {TgFormatting.MarkdownEscape(newLandmark.Coordinates[1].ToString())}]({locationLink})*");

        await this.BroadcastMessage(message.ToString(), escape: false);
        
        var fileName = Path.GetFileName(newLandmark.ImagePath);

        var cachedImg = this._database.GetImageCache(fileName);

        if (cachedImg == null)
        {
            var imagePath = Path.Combine(this.GameTemplate.FilePath, newLandmark.ImagePath);
            var fileStream = System.IO.File.OpenRead(imagePath);
            
            var msg = await this._telegramBot.Client.SendPhoto(this.Game.TelegramGroupId, new InputFileStream(fileStream));

            var photoId = msg.Photo.Last().FileId;
            
            this._database.InsertImageCache(new ImageCache() { FileName = fileName, TgFileId = photoId});
        }
        else
        {
            await this._telegramBot.Client.SendPhoto(this.Game.TelegramGroupId, cachedImg.TgFileId);
        }
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

        if (victim.PlayerGameStateData.HealthPoints < 1)
        {
            await this.SendPlayerMessage(tagger.Id, "\u26a0\ufe0f Tag failed. Targeted player already dead.");
            return;
        }
        
        var tag = new PlayerTag(tagger.Id, victim.Id);

        // check tag status of victim and tagger because of the freeze and protection phase

        if (tagger.PlayerGameStateData.TagStatus == EPlayerTagStatus.Frozen)
        {
            await this.SendPlayerMessage(taggerId,
                "\u26d4 You are currently not allowed to tag others since you were recently tagged and are in freeze");
            await this.BroadcastMessage(
                $"\u26d4 The tag from {tagger.TelegramMention} on {victim.TelegramMention} is invalid since {tagger.TelegramMention} is in freeze from a recent tag", escape: false);
            return;
        }

        if (victim.PlayerGameStateData.TagStatus != EPlayerTagStatus.Default)
        {
            await this.SendPlayerMessage(taggerId,
                $"\u26d4 You are currently not allowed to tag {victim.TelegramMention} because they were already recently tagged", escape: false);
            await this.BroadcastMessage(
                $"\u26d4 The tag from {tagger.TelegramMention} on {victim.TelegramMention} is invalid since {victim.TelegramMention} was already recently tagged", escape: false);
            return;
        }
        
        // fire event
        var eventArgs = new TagEventArgs(tag);
        this.OnPlayerTag.Invoke(this, eventArgs);
        
        if (eventArgs.Cancel)
        {
            await this.BroadcastMessage(
                $"\u274e The tag of {tagger.TelegramMention} on {victim.TelegramMention} was canceled due to a power up of them", escape: false);
            return;
        }
        
        // execute tag
        this.Game.GameStateData.PlayerTags.Add(tag);
        victim.PlayerGameStateData.HealthPoints -= tag.Damage;

        victim.PlayerGameStateData.TagStatus = EPlayerTagStatus.Frozen;
        
        this.OnSuccessfulPlayerTag.Invoke(this, new SuccessfulTagEventArgs(tag));
        
        // protection
        var frozenDate = DateTime.Now.Add((TimeSpan)this.GameData.TagFreeze).ToString("HH:mm:ss");
        var protectedDate = DateTime.Now.Add((TimeSpan)this.GameData.AfterTagProtection).ToString("HH:mm:ss");

        if (victim.PlayerGameStateData.HealthPoints > 0)
        {
            await this.SendPlayerMessage(victim.Id, $"\ud83d\udee1\ufe0f You are now frozen until {frozenDate} and you shall not move and cannot tag others.\n You are also protected until {protectedDate} and can't be tagged by others.");
        }
        else
        {
            await this.SendPlayerMessage(victim.Id, "\ud83d\udc80 You are now dead");
        }

        await this.BroadcastMessage($"\ud83d\udea9 Player {tagger.TelegramMention} successfully tagged {victim.TelegramMention}", escape: false);
    }

    /// <summary>
    /// Revert a tag (including power ups)
    /// </summary>
    /// <param name="TagId">Id of the tag</param>
    /// <returns>successful</returns>
    public async Task<bool> UndoTag(Guid TagId)
    {
        var tag = this.Game.GameStateData.PlayerTags.FirstOrDefault(pt => pt.TagId.Equals(TagId));
        
        if (tag == null) return false;
        
        var tagger = this.GetPlayerById(tag.TaggerId);
        var victim = this.GetPlayerById(tag.VictimId);
        
        if (tagger == null || victim == null) return false;
        
        victim.PlayerGameStateData.HealthPoints += tag.Damage;

        foreach (var powerUp in tag.AppliedPowerUps)
        {
            await PowerUpUtils.RevertPowerUp(powerUp, tag, this);
        }
        
        this.Game.GameStateData.PlayerTags.Remove(tag);

        return true;
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

        var randomPowerUp = this._powerUpSelector.SelectRandomItem();
        var powerUp = PowerUpUtils.GetPowerUpInstance(randomPowerUp.Item, this, claimer.Id);
        
        claimer.PlayerGameStateData.Powerups.Add(powerUp);
        
        lm.Claimed = true;
        
        await this.BroadcastMessage($"\ud83c\udf1f {claimer.TelegramMention} claimed the PowerUp at the current Landmark \"{TgFormatting.MarkdownEscape(lm.Name)}\"", escape: false);

        await this.SendPlayerMessage(claimerId, $"\ud83c\udf1f You have sucessfully claimed the Landmark \"{lm.Name}\" obtained the \"{powerUp.Name}\" power up");
        
        this.OnLandmarkClaim.Invoke(this, new PowerUpUseEventArgs(powerUp, claimer.Id));
        
        return true;
    }

    /// <summary>
    /// Give a specific power up to a specific player
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="desiredPowerUp"></param>
    /// <returns>success</returns>
    public bool GivePlayerPowerUp(Guid playerId, EPowerUp desiredPowerUp)
    {
        var claimer = this.GetPlayerById(playerId);
        
        if (claimer == null) return false;
        
        var powerUp = PowerUpUtils.GetPowerUpInstance(desiredPowerUp, this, claimer.Id);
        claimer.PlayerGameStateData.Powerups.Add(powerUp);

        return true;
    }

    /// <summary>
    /// Activate a PowerUp for a Player. If the player already has one active, the active one expires immediately in favor of the new one
    /// </summary>
    /// <param name="playerId">Id of the player</param>
    /// <param name="powerUpId">Id of the power up</param>
    /// <returns>true if usage was successful, false if it was not used successfully</returns>
    public async Task<bool> UsePowerUp(Guid playerId, Guid powerUpId, string? input = null)
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
        
        powerUp.Use(input);
        
        OnSuccessfulPowerUpUse.Invoke(this, powerUp.Id);
        
        return true;
    }

    public void CheckIfPlayerDead(object? sender, SuccessfulTagEventArgs e)
    {
        var victim = this.GetPlayerById(e.PlayerTag.VictimId);
        
        if (victim.PlayerGameStateData.HealthPoints < 1)
        {
            victim.PlayerGameStateData.TagStatus = EPlayerTagStatus.Default;
            this.BroadcastMessage($"\ud83d\udc80 The player {victim.TelegramMention} is now dead", escape: false);
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
        
        await this.BroadcastMessage($"\ud83c\udfc6 The player {player.TelegramMention} has won the game", escape: false);
        
        await this.PrintScoreboard();
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

    public async Task PrintScoreboard()
    {
        var text = new StringBuilder();

        text.AppendLine("*Final player stats:*\n");

        foreach (var player in this.Players)
        {
            text.AppendLine($"*{player.TelegramMention}:*");

            text.AppendLine($"\ud83d\udc9a Health points\\: *{player.PlayerGameStateData.HealthPoints}*");
            text.AppendLine($"\ud83c\udf1f Power ups\\: *{player.PlayerGameStateData.Powerups.Count}*");
            
            var tags = this.Game.GameStateData.PlayerTags.Count(t => t.TaggerId.Equals(player.Id));
            text.AppendLine($"\ud83d\udea9 Tags\\: *{tags}*");
            text.AppendLine("");
        }
        
        await this.BroadcastMessage(text.ToString(), escape: false);
    }
}