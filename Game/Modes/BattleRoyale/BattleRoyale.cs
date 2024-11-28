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
    
    public BattleRoyaleGamemode(GameTemplate template, BattleRoyaleGameData data, long telegramGroupId, IServiceProvider services) : base(template, telegramGroupId, services)
    {
        this.TimeBetweenDrops = data.TimeBetweenDrops;
        this.Game.GameStateData.Landmarks = new(data.Landmarks);
        
        var commandService = services.GetService<ICommandService>();
        this._telegramBot = services.GetService<ITelegramBotService>();
        
        commandService.AddCommand<TagCommand>();

        this.GameTimer.OnTick += this.Tick;
    }
    
    private void Tick(object sender, EventArgs e)
    {
        this.CheckForNextLandmark();
    }

    private void CheckForNextLandmark()
    {
        if (this.Game.Status != EGameStatus.Running) return;

        if (DateTime.Now < this.Game.GameStateData.LastTimeDropped.Add(this.TimeBetweenDrops)) return;

        this.NewLandmark();
    }

    private void NewLandmark()
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

        this.BroadcastMessage(message.ToString());
        
        var fileStream = System.IO.File.OpenRead(newLandmark.Image);
        this._telegramBot.Client.SendPhoto(this.Game.TelegramGroupId, new InputFileStream(fileStream));
    }
}