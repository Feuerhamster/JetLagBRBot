using System.Text;
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
    public override Task Execute(Message msg, UpdateType type)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);

        if (currentGame == null)
        {
            bot.Client.SendMessage(msg.Chat.Id, "\u26a0\ufe0f Invalid Command: Gamemode not found");
        }
        
        var text = new StringBuilder();
        
        return Task.CompletedTask;
    }
}