using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class PowerUpLock(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    protected override int? TimerDurationMinutes { get; } = 20;

    public override string Name { get; } = "PowerUpLock";

    public override string Description { get; } =
        "No one except you can activate power ups for the next 20 minutes. PowerUps who are already active are unaffected.";
    
    
    public override void Use(string? input)
    {
        base.Use();
        
        this.Gamemode.OnPowerUpUse += this.OnPowerUpUse;

        this.Gamemode.BroadcastMessage(
            $"\ud83d\udeab For the next {this.TimerDurationMinutes} minutes, no one can activate power ups. Already active power ups are unaffected.");
        this.Gamemode.SendPlayerMessage(this.OwnerId,
            $"\ud83c\udf1f Your {this.Name} PowerUp is now active for {this.TimerDurationMinutes} minutes!");
    }

    protected override void OnTimerFinished(object? sender, EventArgs e)
    {
        this.Gamemode.OnPowerUpUse -= this.OnPowerUpUse;
        this.Expire();
        
        this.Gamemode.BroadcastMessage(
            $"\u2705 PowerUps can now be activated again");
    }

    private void OnPowerUpUse(object? sender, PowerUpUseEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;

        if (!e.PlayerId.Equals(this.OwnerId)) return;
        
        e.Cancel = true;
    }
}