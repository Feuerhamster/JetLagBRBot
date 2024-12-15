using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Commands;

public class ClaimCommand(ITelegramBotService bot, IGameManagerService gameManagerService) : CustomBotCommandBase(bot)
{
    public override string Command { get; } = "claim";
    public override string Description { get; } = "Claim the current landmark and get the power up";
    public override ICustomBotCommandConstraint[] Constraints { get; } =
    [
        new OnlyPlayersWhoJoinedConstraint(),
        new OnlyGameModeConstraint(BattleRoyaleGamemode.GameModeName)
    ];
    
    public async override Task Execute(Message msg, UpdateType type)
    {
        var keyboard = new InlineKeyboardChoiceFactory(this.Command);
        
        keyboard.AddChoice("Yes", bool.TrueString);
        keyboard.AddChoice("No", bool.FalseString);
        
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

        if (p == null)
        {
            await bot.Client.DeleteMessage(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id);
            return;
        }
        
        var res = await currentGame.ClaimLandmark(p.Id);

        if (res == false)
        {
            await currentGame.SendPlayerMessage(p.Id, "\u26a0\ufe0f Failed to claim the landmark. Maybe it is already claimed?");
        }
        
        await bot.Client.DeleteMessage(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id);
    }
}