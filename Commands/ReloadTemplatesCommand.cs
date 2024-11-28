using System.Text;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class ReloadTemplatesCommand(IGameTemplateService templateService, ITelegramBotService telegramBotService) : CustomBotCommandBase(telegramBotService)
{
    public override string Command { get; } = "reload_templates";
    public override string Description { get; } = "Reload all game template files";
    public override async Task Execute(Message msg, UpdateType type)
    {
        List<string> reloadLog = templateService.ReloadTemplates();

        StringBuilder text = new StringBuilder();
        text.AppendLine("Reloading template files...");
        text.AppendLine();
        foreach (var line in reloadLog)
        {
            text.AppendLine(line);
        }
        
        telegramBotService.Client.SendMessage(msg.Chat.Id, text.ToString());
    }
}