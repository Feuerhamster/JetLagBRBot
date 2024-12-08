using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class ExtraLifePowerUp(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    protected override string Name { get; } = "Extra Life";
    protected override string Description { get; } = "Instantly adds an extra life to your health points.";

    public override void Use()
    {
        if (this.Status != EPowerUpStatus.Inactive) return;
        
        this.Status = EPowerUpStatus.Active;
        
        var p = this.Gamemode.Players.Find(p => p.Id.Equals(this.OwnerId));
        if (p != null) p.PlayerGameStateData.HealthPoints += 1;

        this.Expire();
    }
}