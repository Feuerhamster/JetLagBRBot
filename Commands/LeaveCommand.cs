using JetLagBRBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class LeaveCommand : ICustomBotCommand
{
    public string Command { get; } = "leave";
    public string Description { get; } = "leave the current gfame";
    public Task Execute(Message msg, UpdateType type)
    {
        throw new NotImplementedException();
    }

    public Task OnCallbackQuery(Update update, string? payloadData)
    {
        throw new NotImplementedException();
    }
}