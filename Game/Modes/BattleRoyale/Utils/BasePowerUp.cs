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
    
    public virtual EPowerUpInput Input { get; } = EPowerUpInput.None;

    protected ManagedTimer Timer { get; private set; } = new(new TimeSpan(0,0, 0));
    
    public virtual void Use(string? input = null)
    {
        this.Status = EPowerUpStatus.Active;

        // if duration is set, power up wants to use the timer
        if (this.TimerDurationMinutes != null)
        {
            this.Timer.Duration = new TimeSpan(0, (int)this.TimerDurationMinutes, 0);
            this.Timer.OnFinished += this.OnTimerFinished;
            this.Timer.Start();
        }
    }
    
    protected virtual void OnTimerFinished(object? sender, EventArgs e)
    {
        this.Expire();
    }
    
    /// <summary>
    /// Expire the power up so it is depleted and no longer usable
    /// </summary>
    public void Expire()
    {
        this.Status = EPowerUpStatus.Expired;
        this.Gamemode.SendPlayerMessage(this.OwnerId, $"\u2b50 Your power up \"{this.Name}\" is now expired");
    }
}