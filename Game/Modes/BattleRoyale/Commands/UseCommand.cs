using JetLagBRBot.Game.Modes.BattleRoyale.Utils;
using JetLagBRBot.Models;
using JetLagBRBot.Services;
using JetLagBRBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Commands;

public class UseCommand(ITelegramBotService bot, IGameManagerService gameManagerService) : CustomBotCommandBase(bot)
{
    public override string Command { get; } = "use";
    public override string Description { get; } = "Use/activate a power up";
    public override ICustomBotCommandConstraint[] Constraints { get; } =
    [
        new OnlyPlayersWhoJoinedConstraint(),
        new OnlyGameModeConstraint(BattleRoyaleGamemode.GameModeName)
    ];

    public async override Task Execute(Message msg, UpdateType type)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);
        
        var keyboard = new InlineKeyboardChoiceFactory(this.Command);
        
        var player = currentGame.Players.FirstOrDefault(p => p.TelegramId == msg.From.Id);
        
        foreach (var powerUp in player.PlayerGameStateData.Powerups.Where(p => p.Status == EPowerUpStatus.Inactive))
        {
            keyboard.AddChoice(powerUp.Name, TgFormatting.EncodeCallbackPayloadData([powerUp.Id.ToString()]));
        }
        
        bot.Client.SendMessage(
            msg.Chat.Id,
            @"Please select the power up you want to activate. Warning: If you have a power up currently active, your active one will be depleted. You can only have one power up at the time active!",
            replyMarkup: keyboard.GetKeyboardMarkup()
        );
    }
    
    public override async Task OnCallbackQuery(Update update, string? payloadData)
    {
        var payload = TgFormatting.DecodeCallbackPayloadData(payloadData);

        if (payload.Length == 1)
        {
            await this.ProcessPowerUpSelection(update, new Guid(payload[0]));
        } else if (payload.Length == 2)
        {
            await this.UsePowerUp(update, ShortGuidHelper.GetGuid(payload[0]), ShortGuidHelper.GetGuid(payload[1]).ToString());
        }
        
        await bot.Client.DeleteMessage(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id);
    }

    private async Task UsePowerUp(Update update, Guid powerUpId, string? input = null)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);
        
        var player = currentGame.Players.FirstOrDefault(p => p.TelegramId == update.CallbackQuery.From.Id);
        
        var success = await currentGame.UsePowerUp(player.Id, powerUpId, input);

        if (!success)
        {
            currentGame.SendPlayerMessage(player.Id, $"\u26a0\ufe0f Failed to activate powerup");
        }
    }

    private async Task ProcessPowerUpSelection(Update update, Guid powerUpId)
    {
        var currentGame =
            gameManagerService.GetCurrentGame<BattleRoyaleGamemode>(BattleRoyaleGamemode.GameModeName);
        
        var currentPlayer = currentGame.Players.FirstOrDefault(p => p.TelegramId == update.CallbackQuery.From.Id);

        var powerUp = currentPlayer.PlayerGameStateData.Powerups.FirstOrDefault(p => p.Id.Equals(powerUpId));
        
        if (powerUp.Input == EPowerUpInput.PlayerId)
        {
            var keyboard = new InlineKeyboardChoiceFactory(this.Command);
        
            foreach (var player in currentGame.Players)
            {
                var payload = TgFormatting.EncodeCallbackPayloadData([
                    ShortGuidHelper.GetShortId(powerUpId), ShortGuidHelper.GetShortId(player.Id)
                ]);
                keyboard.AddChoice(player.Nickname, payload);
            }
        
            await bot.Client.SendMessage(
                update.CallbackQuery.Message.Chat.Id,
                @"Please select the player you want to target",
                replyMarkup: keyboard.GetKeyboardMarkup()
            );
        }
        else
        {
            await this.UsePowerUp(update, powerUpId);
        }
    }
}