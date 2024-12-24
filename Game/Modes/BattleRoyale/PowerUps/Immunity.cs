using System.Timers;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class Immunity(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    protected override int? TimerDurationMinutes { get; } = 5;

    public override string Name { get; } = "Immunity";

    public override string Description { get; } =
        "You cannot be tagged for 5 minutes. You can still get targeted by other power ups.";
    
    
    public override async Task  Use(string? input)
    {
        await base.Use();
        
        this.Gamemode.OnPlayerTag += this.OnPlayerTag;

        await this.Gamemode.SendPlayerMessage(this.OwnerId,
            $"\ud83c\udf1f Your {this.Name} PowerUp is now active for {this.TimerDurationMinutes} minutes!");
    }

    protected override void OnTimerFinished()
    {
        this.Gamemode.OnPlayerTag -= this.OnPlayerTag;
        this.Expire();
    }

    private void OnPlayerTag(object? sender, TagEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;

        if (!e.PlayerTag.VictimId.Equals(this.OwnerId)) return;
        
        e.Cancel = true;
    }
}