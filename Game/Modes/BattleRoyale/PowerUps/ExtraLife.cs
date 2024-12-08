using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class ExtraLifePowerUp(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId, "Extra Life")
{
    public override void Use()
    {
        var p = this.Gamemode.Players.Find(p => p.Id.Equals(this.OwnerId));
        if (p != null) p.PlayerGameStateData.HealthPoints += 1;

        this.Expire();
    }
}