using JetLagBRBot.Game;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Models;

public interface ICustomBotCommandConstraint
{
    public Task<bool> Execute(IServiceProvider serviceProvider, Message msg);
}

/// <summary>
/// Checks if the command is executed from a specific chat type
/// </summary>
/// <param name="chatType"></param>
public class ChatTypeConstraint : ICustomBotCommandConstraint
{
    private ChatType[] ChatTypes { get; set; }
    
    public ChatTypeConstraint(ChatType chatType) {
        ChatTypes = new[] { chatType };
    }

    public ChatTypeConstraint(params ChatType[] chatTypes)
    {
        ChatTypes = chatTypes;
    }
    
    public async Task<bool> Execute(IServiceProvider serviceProvider, Message msg)
    {
        var botService = serviceProvider.GetService<ITelegramBotService>();
        
        if (!ChatTypes.Contains(msg.Chat.Type))
        {
            await botService.Client.SendMessage(
                msg.Chat.Id,
                "\u26a0\ufe0f This command cannot be executed in this kind of chat"
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
    public async Task<bool> Execute(IServiceProvider serviceProvider, Message msg)
    {
        var botService = serviceProvider.GetService<ITelegramBotService>();
        
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

public class OnlyPlayersWhoJoinedConstraint : ICustomBotCommandConstraint
{
    public async Task<bool> Execute(IServiceProvider serviceProvider, Message msg)
    {
        var botService = serviceProvider.GetService<ITelegramBotService>();

        var game = serviceProvider.GetRequiredService<IGameManagerService>().GetCurrentGame<IBaseGame>(null);

        if (game != null && !game.HasPlayer(msg.From.Id))
        {
            await botService.Client.SendMessage(
                msg.Chat.Id,
                "You are not in the current game"
            );
            return false;
        }

        return true;
    }
}

public class OnlyGameModeConstraint(string gameMode) : ICustomBotCommandConstraint
{
    public async Task<bool> Execute(IServiceProvider serviceProvider, Message msg)
    {
        try
        {
            var game = serviceProvider.GetRequiredService<IGameManagerService>().GetCurrentGame<IBaseGame>(gameMode);
            return game != null;
        }
        catch (Exception e)
        {
            return false;
        }
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
