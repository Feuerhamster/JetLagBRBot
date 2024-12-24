using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Models;
using JetLagBRBot.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class Stop(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    public override string Name { get; } = "Stop";

    public override string Description { get; } =
        $"The targeted player has to stop and shall not move for the next 5 minutes.";

    protected override int? TimerDurationMinutes { get; } = 5;

    public override EPowerUpInput Input => EPowerUpInput.PlayerId;

    private Player<PlayerOrTeamStateData> Target { get; set; }
    
    public override async Task  Use(string? input)
    {
        await base.Use();

        var targetId = Guid.Empty;
        if (!Guid.TryParse(input, out targetId)) return;
        
        this.Target = this.Gamemode.Players.FirstOrDefault(p => p.Id.Equals(targetId));

        if (this.Target == null) return;
        
        var player = this.Gamemode.GetPlayerById(this.OwnerId);

        await this.Gamemode.BroadcastMessage($"\u26d4 {player.TelegramMention} has used the \"Stop\" power up on {this.Target.TelegramMention}!\n{this.Target.TelegramMention} has to stop now for {this.TimerDurationMinutes}");
        await this.Gamemode.SendPlayerMessage(this.Target.Id, $"\u26d4 {player.TelegramMention} has used the \"Stop\" power up on you. You have to stop now for {this.TimerDurationMinutes} minutes. If you are in public transport right now, exit on the next possible opportunty and stay there.");
    }

    protected override void OnTimerFinished()
    {
        this.Gamemode.BroadcastMessage($"\u2705 {this.Target.TelegramMention} is free to move again!");
        this.Gamemode.SendPlayerMessage(this.Target.Id, $"\u2705 You are now allowed to move again!");
        
        base.Expire();
    }
}