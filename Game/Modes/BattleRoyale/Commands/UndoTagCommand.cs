using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Commands;

public class UndoTagCommand(ITelegramBotService bot, IGameManagerService gameManagerService) : CustomBotCommandBase(bot)
{
    public override string Command { get; } = "undo_tag";
    public override string Description { get; } = "Undo a previous tag";

    public override ICustomBotCommandConstraint[] Constraints { get; } =
    [
        new OnlyGameModeConstraint(BattleRoyaleGamemode.GameModeName),
        new ChatTypeConstraint(ChatType.Group, ChatType.Supergroup),
        new OnlyGroupAdminConstraint()
    ];

    public async override Task Execute(Message msg, UpdateType type)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);

        var keyboard = new InlineKeyboardChoiceFactory(this.Command);

        const int showLastTagsInMinutes = 10;
        
        var lastTags = currentGame.Game.GameStateData.PlayerTags
            .Where(t => t.TagTime.AddMinutes(showLastTagsInMinutes) > DateTime.Now)
            .OrderByDescending(t => t.TagTime);
        
        foreach (var tag in lastTags)
        {
            var tagger = currentGame.GetPlayerById(tag.TaggerId);
            var victim = currentGame.GetPlayerById(tag.VictimId);
            
            var text = $"{tagger.Nickname} -> {victim.Nickname} @ {tag.TagTime:HH:mm:ss}";
            
            keyboard.AddChoice(text, tag.TagId.ToString());
        }

        await bot.Client.SendMessage(
            msg.Chat.Id,
            "\u23ee\ufe0f Please select a tag to undo.",
            replyMarkup: keyboard.GetKeyboardMarkup());
    }

    public override async Task OnCallbackQuery(Update update, string? payloadData)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);

        var tagId = Guid.Empty;
        if (!Guid.TryParse(payloadData, out tagId)) return;
        
        var tag = currentGame.Game.GameStateData.PlayerTags.FirstOrDefault(t => t.TagId.Equals(tagId));

        if (tag == null)
        {
            // this should never happen
            await bot.Client.SendMessage(update.CallbackQuery.Message.Chat.Id, "\u26a0\ufe0f Failed to locate tag object");
            return;
        }
        
        var tagger = currentGame.GetPlayerById(tag.TaggerId);
        var victim = currentGame.GetPlayerById(tag.VictimId);
            
        var text = $"{tagger.Nickname} -> {victim.Nickname} @ {tag.TagTime:HH:mm:ss}";
        
        var success = await currentGame.UndoTag(tag.TagId);
        
        if (success)
        {
            await currentGame.BroadcastMessage($"\u23ee\ufe0f A tag was reverted by a game master!\n\nTag: {text}\n\nThe victim is no longer frozen or protected.", md: false, escape: false);
        }
        else
        {
            await bot.Client.SendMessage(update.CallbackQuery.Message.Chat.Id, "\u26a0\ufe0f Failed to revert tag");
        }
        
        await bot.Client.DeleteMessage(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id);
    }
}