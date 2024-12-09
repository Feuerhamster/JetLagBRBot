using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class ExtraLifePowerUp(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    protected override string Name { get; } = "Extra Life";
    protected override string Description { get; } = "Instantly adds an extra life to your health points.";

    public override void Use()
    {
        base.Use();
        
        var p = this.Gamemode.Players.Find(p => p.Id.Equals(this.OwnerId));
        if (p == null) return;

        int oldHp = p.PlayerGameStateData.HealthPoints;
        
        p.PlayerGameStateData.HealthPoints += 1;

        this.Gamemode.SendPlayerMessage(p.Id,
            $"\ud83d\udc9a You had **{oldHp}** health points, now you have **{p.PlayerGameStateData.HealthPoints}** health points!");
        
        this.Expire();
    }
}