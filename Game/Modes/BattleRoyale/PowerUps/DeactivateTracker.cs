using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class DeactivateTracker(BattleRoyaleGamemode gamemode, Guid ownerId) : BasePowerUp(gamemode, ownerId)
{
    public override string Name { get; } = "Deactivate Tracker";

    public override string Description { get; } =
        $"You are allowed to deactivate your live location sharing for 15 minutes.";

    protected override int? TimerDurationMinutes { get; } = 15;

    public override void Use(string? input)
    {
        base.Use();

        this.Gamemode.SendPlayerMessage(this.OwnerId,
            $"\ud83c\udf1f Your {this.Name} PowerUp is now active for {this.TimerDurationMinutes} minutes!");

        var player = this.Gamemode.GetPlayerById(this.OwnerId);
        
        var mention = TgFormatting.UserMention(player.TelegramId, player.Nickname);
        
        this.Gamemode.BroadcastMessage($"\ud83d\udce1 The player {mention} has used the {Name} PowerUp. They are now allowed to deactivate their live location sharing for {this.TimerDurationMinutes} minutes.");
    }
}