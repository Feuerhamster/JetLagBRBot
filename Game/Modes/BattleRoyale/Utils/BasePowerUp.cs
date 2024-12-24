using JetLagBRBot.Models;
using JetLagBRBot.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Utils;

public enum EPowerUpStatus
{
    Inactive,
    Active,
    Expired
}

public enum EPowerUpInput
{
    None,
    PlayerId
}

/// <summary>
/// A class that implements base power up functionality
/// </summary>
/// <param name="gamemode"></param>
/// <param name="ownerId"></param>
///
/// TODO: power ups are currently not serializable - fix in future for savegame functionality
public abstract class BasePowerUp(BattleRoyaleGamemode gamemode, Guid ownerId)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public EPowerUpStatus Status { get; protected set; } = EPowerUpStatus.Inactive;
    
    protected readonly BattleRoyaleGamemode Gamemode = gamemode;
    protected readonly Guid OwnerId = ownerId;
    
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    /// <summary>
    /// If this is not null, the powerup will have a timer that automatically starts when base Use() method is called and expires the power up automatically after it finished
    /// </summary>
    protected virtual int? TimerDurationMinutes { get; } = null;
    
    private bool IsTimedPowerUp => this.TimerDurationMinutes != null && this.TimerDurationMinutes.HasValue;
    
    public DateTime UsageActivationTime { get; private set; } = DateTime.MinValue;
    
    public virtual EPowerUpInput Input { get; } = EPowerUpInput.None;
    
    public virtual async Task Use(string? input = null)
    {
        this.Status = EPowerUpStatus.Active;

        this.UsageActivationTime = DateTime.Now;
        
        // if duration is set, power up wants to use the timer
        if (this.IsTimedPowerUp)
        {
            this.Gamemode.GameTimer.OnTick += this.Tick;
        }
    }
    
    protected virtual async Task OnTimerFinished()
    {
        this.Expire();
    }

    private void Tick(object? sender, EventArgs e)
    {
        if (!this.IsTimedPowerUp || this.Status != EPowerUpStatus.Active) return;

        var duration = new TimeSpan(0, 0, (int)this.TimerDurationMinutes, 0);
        if (ManagedTimer.VerifyTimeIsOver(this.UsageActivationTime, duration))
        {
            this.OnTimerFinished();
        }
    }
    
    /// <summary>
    /// Expire the power up so it is depleted and no longer usable
    /// </summary>
    public async Task Expire()
    {
        if (this.IsTimedPowerUp)
        {
            this.Gamemode.GameTimer.OnTick -= this.Tick;
        }
        
        this.Status = EPowerUpStatus.Expired;
        await this.Gamemode.SendPlayerMessage(this.OwnerId, $"\u2b50 Your power up \"{this.Name}\" is now expired");
    }
}