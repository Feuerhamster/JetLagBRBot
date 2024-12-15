using JetLagBRBot.Game;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class StartCommand(ITelegramBotService telegramBotService)
    : CustomBotCommandBase(telegramBotService)
{
    public override string Command { get; } = "start";
    public override string Description { get; } = "initialize interaction with the bot";

    public override async Task Execute(Message msg, UpdateType type)
    {
        telegramBotService.Client.SendMessage(msg.Chat.Id,
            $"Welcome to the jet lag battle royale bot. Please wait for your game master for further instructions");
    }
}