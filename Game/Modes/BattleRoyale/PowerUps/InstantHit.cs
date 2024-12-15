using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Models;
using JetLagBRBot.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class InstantHit(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    public override string Name { get; } = "InstantHit";

    public override string Description { get; } =
        $"Instantly remove 1 health from a player of your choice. Can't be used on players with only 1 health left.";

    public override EPowerUpInput Input => EPowerUpInput.PlayerId;

    private Player<PlayerOrTeamStateData> Target { get; set; }
    
    public override void Use(string? input)
    {
        base.Use();

        var targetId = Guid.Empty;
        if (!Guid.TryParse(input, out targetId)) return;
        
        this.Target = this.Gamemode.Players.FirstOrDefault(p => p.Id.Equals(targetId));

        if (this.Target == null) return;

        if (this.Target.PlayerGameStateData.HealthPoints <= 1)
        {
            this.Gamemode.SendPlayerMessage(this.OwnerId, $"\u26a0\ufe0f Invalid choice. This player only has one health point left. Please choose someone else.");
            return;
        }
        
        this.Target.PlayerGameStateData.HealthPoints -= 1;

        var owner = this.Gamemode.GetPlayerById(this.OwnerId);

        this.Gamemode.BroadcastMessage(
            $"\ud83d\udca3 The player {owner.TelegramMention} used the \"Instant hit\" power up to remove 1 hp from {this.Target.TelegramMention}");

        this.Gamemode.SendPlayerMessage(this.Target.Id,
            $"\ud83d\udca3 You have been instantly hit by {owner.TelegramMention} and lost 1 health point.");
        
        this.Expire();
    }
}