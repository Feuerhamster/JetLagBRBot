using JetLagBRBot.Models;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Utils;

public enum EPowerUpStatus
{
    Inactive,
    Active,
    Expired
}

public abstract class BasePowerUp(BattleRoyaleGamemode gamemode, Guid ownerId, string name)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public EPowerUpStatus Status { get; private set; } = EPowerUpStatus.Inactive;
    
    protected readonly BattleRoyaleGamemode Gamemode = gamemode;
    protected readonly Guid OwnerId = ownerId;
    protected readonly string Name = name;

    public abstract void Use();
    
    public void Expire()
    {
        this.Status = EPowerUpStatus.Expired;
        this.Gamemode.SendPlayerMessage(this.OwnerId, $"\u2b50 Your power up \"{this.Name}\" is now expired");
        var p = this.Gamemode.Players.First(p => p.Id.Equals(this.OwnerId));
    }
}