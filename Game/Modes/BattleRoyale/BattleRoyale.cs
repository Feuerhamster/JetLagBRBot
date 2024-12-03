using System.Text;
using JetLagBRBot.Game;
using JetLagBRBot.GameModes.BattleRoyale.Commands;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JetLagBRBot.GameModes.BattleRoyale;

public class BattleRoyaleGamemode : BaseGame<GameStateData, PlayerOrTeamStateData, PlayerOrTeamStateData>
{
    public const string GameModeName = "BattleRoyale";

    private readonly TimeSpan TimeBetweenDrops;
    
    private readonly ITelegramBotService _telegramBot;
    
    private readonly IServiceProvider _services;
    
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
        var victim = this.Players.FirstOrDefault(p => p.Id.Equals(victimId));
        var tagger = this.Players.FirstOrDefault(p => p.Id.Equals(taggerId));
        
        if (victim == null || tagger == null) return;
        
        // check for powerups
        var victimPowerup =
            victim.PlayerGameStateData.Powerups.FirstOrDefault(p =>
                p is { Activator: EPowerupActivator.OnTagged, IsActive: true });
        
        var taggerPowerup =
            tagger.PlayerGameStateData.Powerups.FirstOrDefault(p =>
                p is { Activator: EPowerupActivator.OnTagged, IsActive: true });

        if (victimPowerup != null)
        {
            var cancel = await victimPowerup.Use(this, this._services);

            if (cancel) return;
        }
        
        if (taggerPowerup != null)
        {
            var cancel = await taggerPowerup.Use(this, this._services);

            if (cancel) return;
        }
        
        this.Game.GameStateData.PlayerTags.Add(new KeyValuePair<Guid, Guid>(tagger.Id, victim.Id));
    }
}