using System.Timers;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class ImmunityPowerUp(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    private const int DurationInMinutes = 5;
    
    protected override string Name { get; } = "Immunity";

    protected override string Description { get; } =
        $"You cannot be tagged for {DurationInMinutes} minutes. You can still get targeted by other power ups.";
    
    private ManagedTimer Timer { get; } = new ManagedTimer(new TimeSpan(0, DurationInMinutes, 0));
    
    public override void Use()
    {
        this.Timer.Start();
        this.Status = EPowerUpStatus.Active;

        this.Timer.OnFinished += OnTimerFinished;
        
        this.Gamemode.OnPlayerTag += this.OnPlayerTag;

        this.Gamemode.SendPlayerMessage(this.OwnerId,
            $"\ud83c\udf1f Your {this.Name} PowerUp is now active for {DurationInMinutes} minutes!");
    }

    private void OnTimerFinished(object? sender, EventArgs e)
    {
        this.Gamemode.OnPlayerTag -= this.OnPlayerTag;
        this.Expire();
    }

    private void OnPlayerTag(object? sender, TagEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;
        
        e.Cancel = true;
    }
}