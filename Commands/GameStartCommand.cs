using JetLagBRBot.Game;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Commands;

public class GameStartCommand(IGameManagerService gameManagerService, ITelegramBotService telegramBotService) : CustomBotCommandBase(telegramBotService)
{
    public override string Command { get; } = "start_game";
    public override string Description { get; } = "Starts the game that is currently initialized in the group";

    public override ICustomBotCommandConstraint[] Constraints { get; } =
    [
        new ChatTypeConstraint(ChatType.Group),
        new OnlyGroupAdminConstraint()
    ];

    public override async Task Execute(Message msg, UpdateType type)
    {
        var CurrentGame =
            gameManagerService.GetCurrentGame<IBaseGame>(null);

        if (CurrentGame == null)
        {
            telegramBotService.Client.SendMessage(msg.Chat.Id,
                $"No current game initialized. Please talk to your game master");
            return;
        }
        
        CurrentGame.StartGame();
    }
}