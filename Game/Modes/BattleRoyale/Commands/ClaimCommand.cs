using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Commands;

public class ClaimCommand(ITelegramBotService bot, IGameManagerService gameManagerService) : CustomBotCommandBase(bot)
{
    public override string Command { get; } = "tag";
    public override string Description { get; } = "Tags a player";
    public async override Task Execute(Message msg, UpdateType type)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);

        if (currentGame == null)
        {
            await bot.Client.SendMessage(msg.Chat.Id, "\u26a0\ufe0f Invalid Command: Gamemode not found");
            return;
        }
        
        var keyboard = new InlineKeyboardChoiceFactory(this.Command);
        
        foreach (var player in currentGame.Players)
        {
            keyboard.AddChoice("Yes", bool.TrueString);
            keyboard.AddChoice("No", bool.FalseString);
        }
        
        await bot.Client.SendMessage(
            msg.Chat.Id,
            "Are you at the current landmark, have taken a photo of you in front of the landmark and posted it into the game chat group?",
            replyMarkup: keyboard.GetKeyboardMarkup()
        );
    }
    
    public override async Task OnCallbackQuery(Update update, string? payloadData)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);

        if (payloadData != bool.TrueString)
        {
            await bot.Client.DeleteMessage(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id);
            return;
        }

        var p = currentGame.Players.FirstOrDefault(p => p.TelegramId == update.CallbackQuery.From.Id);

        if (p == null) return;
        
        await currentGame.ClaimLandmark(p.Id);
    }
}