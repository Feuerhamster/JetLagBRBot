using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Commands;

public class TagCommand(ITelegramBotService bot, IGameManagerService gameManagerService) : CustomBotCommandBase(bot)
{
    public override string Command { get; } = "tag";
    public override string Description { get; } = "Tags a player";

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
        
        var keyboard = new InlineKeyboardChoiceFactory(this.Command);

        var myself = currentGame.Players.First(p => p.TelegramId == msg.From.Id);
        
        foreach (var player in currentGame.Players)
        {
            if (player.Id.Equals(myself.Id)) continue;
            
            keyboard.AddChoice(player.Nickname, player.Id.ToString());
        }
        
        bot.Client.SendMessage(
            msg.Chat.Id,
            @"Please select the player you wanna tag\. *You have to post a photo of them on which they can be identified into the game chat group\!*",
            replyMarkup: keyboard.GetKeyboardMarkup(),
            parseMode: ParseMode.MarkdownV2
        );
    }
    
    public override async Task OnCallbackQuery(Update update, string? payloadData)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);

        var victim = currentGame.GetPlayerById(new Guid(payloadData));
        
        if (victim == null)
        {
            await bot.Client.SendMessage(
                update.CallbackQuery.Message.Chat.Id,
                $"\u26a0\ufe0f Failed to select victim"
            );
            return;
        }
        
        var tagger = currentGame.Players.FirstOrDefault(p => p.TelegramId == update.CallbackQuery.From.Id);

        if (tagger == null)
        {
            await bot.Client.SendMessage(
                update.CallbackQuery.Message.Chat.Id,
                $"\u26a0\ufe0f Failed to fetch tagger"
            );
            return;
        }
        
        await currentGame.TagPlayer(tagger.Id, victim.Id);
        
        await bot.Client.DeleteMessage(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id);
    }
}