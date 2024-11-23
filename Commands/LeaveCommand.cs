using JetLagBRBot.Game;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class LeaveCommand(IGameManagerService gameManagerService, ITelegramBotService telegramBotService) : ICustomBotCommand
{
    public string Command { get; } = "leave";
    public string Description { get; } = "leave the current game";
    public async Task Execute(Message msg, UpdateType type)
    {
        var CurrentGame =
            gameManagerService.GetCurrentGame<BaseGame<object, object, object>>(null);
        
        if (CurrentGame == null)
        {
            telegramBotService.Client.SendMessage(msg.Chat.Id,
                $"No current game initialized. Please talk to your game master");
            return;
        }
        
        bool hasLeft = CurrentGame.LeavePlayer(msg.From.Id);


        telegramBotService.Client.SendMessage(msg.Chat.Id,
            hasLeft
                ? $"Successfully left the game \"{CurrentGame.GameTemplate.Config.Name}\""
                : $"Failed to leave game. Maybe you never joined?");
    }

    public Task OnCallbackQuery(Update update, string? payloadData)
    {
        return Task.CompletedTask;
    }
}