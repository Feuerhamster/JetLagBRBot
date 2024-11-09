using JetLagBRBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class JoinCommand : ICustomBotCommand
{
    public string Command { get; } = "join";
    public string Description { get; } = "Join the current game";
    public Task Execute(Message msg, UpdateType type)
    {
        throw new NotImplementedException();
    }

    public Task OnCallbackQuery(Update update, string? payloadData)
    {
        throw new NotImplementedException();
    }
}