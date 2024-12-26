using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Models;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class Stealing(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    public override string Name { get; } = "Stealing";

    public override string Description { get; } =
        $"Steal one random power up from the inventory of another player.";

    public override EPowerUpInput Input => EPowerUpInput.PlayerId;

    private Player<PlayerOrTeamStateData> Target { get; set; }
    
    public override async Task  Use(string? input)
    {
        await base.Use();

        var targetId = Guid.Empty;
        if (!Guid.TryParse(input, out targetId)) return;
        
        this.Target = this.Gamemode.Players.FirstOrDefault(p => p.Id.Equals(targetId));

        if (this.Target == null) return;

        var targetAvailablePowerUps = this.Target.PlayerGameStateData.Powerups.Where(p => p.Status == EPowerUpStatus.Inactive).ToList();
        
        if (targetAvailablePowerUps.Count == 0)
        {
            await this.Gamemode.SendPlayerMessage(this.OwnerId, $"\u26a0\ufe0f Invalid choice. This player has no power ups that could be stolen.");
            return;
        }
        
        var owner = this.Gamemode.GetPlayerById(this.OwnerId);

        var random = new Random();
        var randomPowerUpIndex = random.Next(0, targetAvailablePowerUps.Count);
        var selectedPowerUp = targetAvailablePowerUps[randomPowerUpIndex];
        
        this.Target.PlayerGameStateData.Powerups.Remove(selectedPowerUp);
        owner.PlayerGameStateData.Powerups.Add(selectedPowerUp);
        
        await this.Gamemode.BroadcastMessage(
            $"\ud83c\udf92 The player {owner.TelegramMention} has stolen a power up from {this.Target.TelegramMention}", escape: false);

        await this.Gamemode.SendPlayerMessage(this.Target.Id,
            $"\ud83c\udf92 The player {owner.TelegramMention} has stolen the \"{selectedPowerUp.Name}\" power up from you", escape: false);
        
        await this.Gamemode.SendPlayerMessage(this.OwnerId, $"\ud83c\udf92 You have sucessfully stolen the \"{selectedPowerUp.Name}\" power up from {this.Target.TelegramMention}", escape: false);
        
        await this.Expire();
    }
}