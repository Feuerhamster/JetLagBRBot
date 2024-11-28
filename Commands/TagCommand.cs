using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class TagCommand(ITelegramBotService telegramBotService) : CustomBotCommandBase(telegramBotService)
{
    public override string Command { get; } = "tag";
    public override string Description { get; } = "Tags a player";
    public override Task Execute(Message msg, UpdateType type)
    {
        throw new NotImplementedException();
    }
}