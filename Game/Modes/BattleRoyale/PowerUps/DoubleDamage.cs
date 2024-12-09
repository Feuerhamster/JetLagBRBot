using System.Timers;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class DoubleDamagePowerUp(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    protected override string Name { get; } = "Double Damage";

    protected override string Description { get; } =
        "The next player you successfully tag gets double the damage of a regular tag.";
    
    private Guid currentTag = Guid.Empty;
    
    public override void Use()
    {
        base.Use();
        
        this.Gamemode.OnPlayerTag += this.OnPlayerTag;
        this.Gamemode.OnSuccessfulPlayerTag += this.OnSuccessfulPlayerTag;

        this.Gamemode.SendPlayerMessage(this.OwnerId,
            $"\ud83c\udf1f Your {this.Name} PowerUp is now active until used");
    }
    
    private void OnPlayerTag(object? sender, TagEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;

        if (!e.PlayerTag.TaggerId.Equals(this.OwnerId)) return;
        
        e.PlayerTag.Damage *= 2;
        this.currentTag = e.PlayerTag.TagId;
    }
    
    private void OnSuccessfulPlayerTag(object? sender, SuccessfulTagEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;

        if (!e.PlayerTag.TaggerId.Equals(this.OwnerId)) return;
        
        if (!this.currentTag.Equals(e.PlayerTag.TagId)) return;
        
        this.Expire();
    }
}