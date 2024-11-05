using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Models;

public interface IBotCommand
{
    public string Command { get; }
    public string Description { get; }
    public Task Execute(Message msg, UpdateType type);
}