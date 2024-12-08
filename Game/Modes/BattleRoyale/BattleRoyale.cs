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

    public event EventHandler<TagEventArgs> OnPlayerTag; 
    
    public event EventHandler<SuccessfulTagEventArgs> OnSuccessfulPlayerTag; 
    
    public BattleRoyaleGamemode(GameTemplate template, BattleRoyaleGameData data, long telegramGroupId, IServiceProvider services) : base(template, telegramGroupId, services)
    {
        this.TimeBetweenDrops = data.TimeBetweenDrops;
        this._services = services;
        this.Game.GameStateData.Landmarks = new(data.Landmarks);
        
        var commandService = services.GetRequiredService<ICommandService>();
        this._telegramBot = services.GetRequiredService<ITelegramBotService>();
        
        commandService.AddCommand<TagCommand>();

        this.GameTimer.OnTick += this.Tick;
    }
    
    private void Tick(object? sender, EventArgs e)
    {
        this.CheckForNextLandmark();
    }

    private void CheckForNextLandmark()
    {
        if (this.Game.Status != EGameStatus.Running) return;

        if (DateTime.Now < this.Game.GameStateData.LastTimeDropped.Add(this.TimeBetweenDrops)) return;

        this.NewLandmark();
    }

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

    public async Task TagPlayer(Guid taggerId, Guid victimId)
    {
        var victim = this.GetPlayerById(victimId);
        var tagger = this.GetPlayerById(taggerId);
        
        if (victim == null || tagger == null) return;
        
        var tag = new PlayerTag(tagger.Id, victim.Id);
        
        // fire event
        var eventArgs = new TagEventArgs(tag);
        this.OnPlayerTag.Invoke(this, eventArgs);

        var taggerMention = TgFormatting.UserMention(tagger.TelegramId, tagger.Nickname);
        var victimMention = TgFormatting.UserMention(victim.TelegramId, victim.Nickname);
        
        if (eventArgs.Cancel)
        {
            await this.BroadcastMessage(
                $"\u274e The tag of {taggerMention} on {victimMention} was canceled due to a power up of them!");
            return;
        }
        
        this.OnSuccessfulPlayerTag.Invoke(this, new SuccessfulTagEventArgs(tag));
        
        this.Game.GameStateData.PlayerTags.Add(tag);

        victim.PlayerGameStateData.HealthPoints -= tag.Damage;
        
        await this.BroadcastMessage($"\ud83d\udea9 Player {taggerMention} successfully tagged {victimMention}!");
    }

    /// <summary>
    /// Claim a landmark
    /// </summary>
    /// <param name="claimerId">Player id of the player who claims the landmark</param>
    /// <returns>true if successful, false if failed</returns>
    public async Task<bool> ClaimLandmark(Guid claimerId)
    {
        var lm = this.Game.GameStateData.CurrentActiveLandmark;
        
        if (lm == null || lm.Claimed)
        {
            return false;
        }

        var claimer = this.GetPlayerById(claimerId);

        if (claimer == null) return false;
        
        // TODO: implement actual powerup claim
        var randomPowerUp = PowerUpUtils.RandomPowerUp();
        var powerUp = PowerUpUtils.GetPowerUp(randomPowerUp, this, claimer.Id);
        
        claimer.PlayerGameStateData.Powerups.Add(powerUp);
        
        this.BroadcastMessage($"\ud83c\udf1f {TgFormatting.UserMention(claimer.TelegramId, claimer.Nickname)} claimed the PowerUp at the current Landmark \"{lm.Name}\"");
        return true;
    }
}