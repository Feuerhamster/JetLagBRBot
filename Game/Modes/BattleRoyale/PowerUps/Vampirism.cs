using System.Timers;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class Vampirism(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    public override string Name { get; } = "Vampirism";

    public override string Description { get; } =
        "If you successfully tag someone, you will get +1 extra HP.";
    
    private Guid currentTag = Guid.Empty;
    
    public override void Use(string? input)
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

        var owner = this.Gamemode.GetPlayerById(this.OwnerId);
        owner.PlayerGameStateData.HealthPoints += 1;
        this.currentTag = e.PlayerTag.TagId;
    }
    
    private void OnSuccessfulPlayerTag(object? sender, SuccessfulTagEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;

        if (!e.PlayerTag.TaggerId.Equals(this.OwnerId)) return;
        
        if (!this.currentTag.Equals(e.PlayerTag.TagId)) return;

        this.Gamemode.SendPlayerMessage(this.OwnerId, $"\ud83e\ude78 You now got +1 HP extra because of vampirism!");
        
        this.Expire();
    }
}