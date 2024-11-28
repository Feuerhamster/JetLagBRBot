using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace JetLagBRBot.Commands;

public class NewGameCommand(ITelegramBotService telegramBotService, IGameTemplateService gameTemplateService, IGameManagerService gameManagerService)
    : CustomBotCommandBase(telegramBotService)
{
    public override string Command { get; } = "newgame";
    public override string Description { get; } = "Create a new game based on a template";

    public override ICustomBotCommandConstraint[] Constraints { get; } =
    [
        new ChatTypeConstraint(ChatType.Group),
        new OnlyGroupAdminConstraint()
    ];

    public override async Task Execute(Message msg, UpdateType type)
    {
        var keyboard = new InlineKeyboardChoiceFactory(this.Command);

        var templates = gameTemplateService.GetTemplateList();

        foreach (var template in templates)
        {
            keyboard.AddChoice(template.Value, template.Key.ToString());
        }
        
        telegramBotService.Client.SendMessage(
            msg.Chat.Id,
            "Please select one of the following templates for your new game",
            replyMarkup: keyboard.GetKeyboardMarkup()
        );
    }

    public async Task OnCallbackQuery(Update update, string? payloadData)
    {
        var t = gameTemplateService.GetGameTemplate(new Guid(payloadData));

        if (t == null)
        {
            await telegramBotService.Client.SendMessage(
                update.CallbackQuery.Message.Chat.Id,
                $"Failed to create game"
            );
            return;
        }
        
        gameManagerService.InitNewGame(t.id, update.Message.Chat.Id);
        
        await telegramBotService.Client.SendMessage(
            update.CallbackQuery.Message.Chat.Id,
            $"Game \"{t.Config.Name}\" successfully created"
        );

        await telegramBotService.Client.DeleteMessage(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id);
    }
}