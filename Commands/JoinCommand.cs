using JetLagBRBot.Game;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class JoinCommandBase(IGameManagerService gameManagerService, ITelegramBotService telegramBotService) : CustomBotCommandBase(telegramBotService)
{
    public override string Command { get; } = "join";
    public override string Description { get; } = "Join the current game";
    
    public async override Task Execute(Message msg, UpdateType type)
    {
        string name = $"{msg.From.FirstName} {msg.From.LastName}";

        var CurrentGame =
            gameManagerService.GetCurrentGame<IBaseGame>(null);

        if (CurrentGame == null)
        {
            telegramBotService.Client.SendMessage(msg.Chat.Id,
                $"No current game initialized. Please talk to your game master");
            return;
        }
        
        var joined = CurrentGame.PlayerJoin(msg.From.Id, name);

        if (joined)
        {
            telegramBotService.Client.SendMessage(msg.Chat.Id,
                $"Successfully joined the game \"{CurrentGame.GameTemplate.Config.Name}\"");
        }
        else
        {
            telegramBotService.Client.SendMessage(msg.Chat.Id,
                $"You are already in the game \"{CurrentGame.GameTemplate.Config.Name}\"");
        }
    }
}