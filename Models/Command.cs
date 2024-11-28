using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Models;

public interface ICustomBotCommandConstraint
{
    public Task<bool> Execute(ITelegramBotService botService, Message msg);
}

/// <summary>
/// Checks if the command is executed from a specific chat type
/// </summary>
/// <param name="chatType"></param>
public class ChatTypeConstraint(ChatType chatType) : ICustomBotCommandConstraint
{
    public async Task<bool> Execute(ITelegramBotService botService, Message msg)
    {
        if (msg.Chat.Type != chatType)
        {
            await botService.Client.SendMessage(
                msg.Chat.Id,
                "A new game can only be created within in a group"
            );
            return false;
        }

        return true;
    }
}

/// <summary>
/// Checks if the command is executed by a group admin
/// </summary>
public class OnlyGroupAdminConstraint : ICustomBotCommandConstraint
{
    public async Task<bool> Execute(ITelegramBotService botService, Message msg)
    {
        var admins = await botService.Client.GetChatAdministrators(msg.Chat.Id);

        if (admins.FirstOrDefault(c => c.User.Id == msg.From.Id) == null)
        {
            await botService.Client.SendMessage(
                msg.Chat.Id,
                "Only group admins can use this command"
            );
            return false;
        }

        return true;
    }
}

/// <summary>
/// Base class for custom bot commands
/// </summary>
/// <param name="telegramBotService"></param>
public abstract class CustomBotCommandBase(ITelegramBotService telegramBotService)
{
    public abstract string Command { get; }
    public abstract string Description { get; }
    
    /// <summary>
    /// Constraints are run before the Execute methods and have the ability to prevent command execution
    /// </summary>
    public virtual ICustomBotCommandConstraint[] Constraints { get; }
    
    public abstract Task Execute(Message msg, UpdateType type);

    public virtual Task OnCallbackQuery(Update update, string? payloadData)
    {
        return Task.CompletedTask;
    }
}
