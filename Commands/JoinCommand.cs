using JetLagBRBot.Game;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class JoinCommand(IGameManagerService gameManagerService, ITelegramBotService telegramBotService) : ICustomBotCommand
{
    public string Command { get; } = "join";
    public string Description { get; } = "Join the current game";
    
    public async Task Execute(Message msg, UpdateType type)
    {
        string name = $"{msg.From.FirstName} {msg.From.LastName}";

        var CurrentGame =
            gameManagerService.GetCurrentGame<BaseGame<object, object, object>>(null);

        if (CurrentGame == null)
        {
            telegramBotService.Client.SendMessage(msg.Chat.Id,
                $"No current game initialized. Please talk to your game master");
            return;
        }
        
        CurrentGame.PlayerJoin(msg.From.Id, name);

        telegramBotService.Client.SendMessage(msg.Chat.Id,
            $"Successfully joined the game \"{CurrentGame.GameTemplate.Config.Name}\"");
    }

    public Task OnCallbackQuery(Update update, string? payloadData)
    {
        return Task.CompletedTask;
    }
}