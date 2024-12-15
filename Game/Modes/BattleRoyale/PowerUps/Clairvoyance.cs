using System.Text;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Models;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class Clairvoyance(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    protected override int? TimerDurationMinutes { get; } = 20;

    public override string Name { get; } = "Clairvoyance";

    public override string Description { get; } =
        "Look what power ups other players have in their inventory and get notified for the next 20 minutes if someone activated a power up.";
    
    public override void Use(string? input)
    {
        base.Use();
        
        this.Gamemode.OnPowerUpUse += this.OnPowerUpUse;
        this.Gamemode.OnLandmarkClaim += this.OnLandmarkClaim;
        
        this.Gamemode.SendPlayerMessage(this.OwnerId,
            $"\ud83c\udf1f Your {this.Name} PowerUp is now active for {this.TimerDurationMinutes} minutes!");

        var text = new StringBuilder();
        text.AppendLine("Here are the power up inventories of all players:\n");
        
        foreach (var player in this.Gamemode.Players)
        {
            text.AppendLine($"{player.Nickname}:");
            
            foreach (var powerUp in player.PlayerGameStateData.Powerups)
            {
                text.AppendLine($"- {powerUp.Name}");
            }

            text.AppendLine();
        }
        
        this.Gamemode.SendPlayerMessage(this.OwnerId, text.ToString());
    }

    protected override void OnTimerFinished(object? sender, EventArgs e)
    {
        this.Gamemode.OnPowerUpUse -= this.OnPowerUpUse;
        this.Gamemode.OnLandmarkClaim -= this.OnLandmarkClaim;
        this.Expire();
    }

    private void OnPowerUpUse(object? sender, PowerUpUseEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;

        var p = this.Gamemode.Players.FirstOrDefault(p => p.Id.Equals(e.PlayerId));
        
        this.Gamemode.SendPlayerMessage(this.OwnerId, $"\ud83d\udd2e {p.Nickname} use the {e.PowerUp.Name} power up");
    }
    
    private void OnLandmarkClaim(object? sender, PowerUpUseEventArgs e)
    {
        if (this.Status != EPowerUpStatus.Active) return;

        var p = this.Gamemode.Players.FirstOrDefault(p => p.Id.Equals(e.PlayerId));
        
        this.Gamemode.SendPlayerMessage(this.OwnerId, $"\ud83d\udd2e {p.Nickname} claimed the {e.PowerUp.Name} power up");
    }
}