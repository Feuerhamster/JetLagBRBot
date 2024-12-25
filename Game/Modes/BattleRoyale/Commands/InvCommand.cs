using System.Text;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Commands;

public class InvCommand(ITelegramBotService bot, IGameManagerService gameManagerService) : CustomBotCommandBase(bot)
{
    public override string Command { get; } = "inventory";
    public override string Description { get; } = "List all your power ups in your inventory and show your current player status";
    public override ICustomBotCommandConstraint[] Constraints { get; } =
    [
        new OnlyPlayersWhoJoinedConstraint(),
        new OnlyGameModeConstraint(BattleRoyaleGamemode.GameModeName),
        new ChatTypeConstraint(ChatType.Private)
    ];

    public async override Task Execute(Message msg, UpdateType type)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);

        if (currentGame == null)
        {
            bot.Client.SendMessage(msg.Chat.Id, "\u26a0\ufe0f Invalid Command: Gamemode not found");
        }
        
        var player = currentGame.Players.FirstOrDefault(p => p.TelegramId == msg.From.Id);

        if (player == null) return;
        
        var text = new StringBuilder();
        
        text.AppendLine($"\ud83d\udc9a Health Points: {player.PlayerGameStateData.HealthPoints}\n");

        text.AppendLine("\ud83c\udf92 *Inventory:*\n");

        var invPowerUps = player.PlayerGameStateData.Powerups.Where(p => p.Status == EPowerUpStatus.Inactive);
        
        foreach (var powerUp in invPowerUps)
        {
            text.AppendLine($"*{powerUp.Name}*: {powerUp.Description}\n");
        }
        
        var activePowerUps = player.PlayerGameStateData.Powerups.Where(p => p.Status == EPowerUpStatus.Active);

        text.AppendLine("\ud83c\udf1f *Active PowerUps:*\n");
        
        foreach (var powerUp in activePowerUps)
        {
            text.AppendLine($"*{powerUp.Name}*: {powerUp.Description}\n");
        }
        
        currentGame.SendPlayerMessage(player.Id, text.ToString());
    }
}