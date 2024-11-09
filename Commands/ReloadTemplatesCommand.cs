using System.Text;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class ReloadTemplatesCommand(IGameTemplateService templateService, ITelegramBotService telegramBotService) : ICustomBotCommand
{
    public string Command { get; } = "reload_templates";
    public string Description { get; } = "Reload all game template files";
    public async Task Execute(Message msg, UpdateType type)
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

    public Task OnCallbackQuery(Update update, string? payloadData)
    {
        return Task.CompletedTask;
    }
}