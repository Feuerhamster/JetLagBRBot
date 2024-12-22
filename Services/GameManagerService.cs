using JetLagBRBot.Commands;
using JetLagBRBot.Game;
using JetLagBRBot.Game.Modes.BattleRoyale;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace JetLagBRBot.Services;

public interface IGameManagerService
{
    public void InitNewGame(Guid templateId, long tgGroupId);

    public G? GetCurrentGame<G>(string? gameMode) where G : class;
    public void LoadCommands();
}

public class GameManagerService(ICommandService commandService, ITelegramBotService telegramBotService, IGameTemplateService gameTemplateService, IServiceProvider serviceProvider) : IGameManagerService
{
    private IBaseGame? CurrentGame { get; set; }

    public void LoadCommands()
    {
        commandService.AddCommand<NewGameCommand>();
        commandService.AddCommand<ReloadTemplatesCommand>();
        commandService.AddCommand<JoinCommandBase>();
        commandService.AddCommand<LeaveCommand>();
        commandService.AddCommand<GameStartCommand>();
        commandService.AddCommand<StopCommand>();
        commandService.AddCommand<ResetCommand>();
        commandService.AddCommand<StartCommand>();
        telegramBotService.UpdateCommands();
    }

    public void InitNewGame(Guid templateId, long tgGroupId)
    {
        var template = gameTemplateService.GetGameTemplate(templateId);

        switch (template.Config.GameMode)
        {
            case BattleRoyaleGamemode.GameModeName:
            {
                var data = gameTemplateService.LoadGameData<BattleRoyaleGameData>(templateId);
                
                this.CurrentGame = new BattleRoyaleGamemode(template, data, tgGroupId, serviceProvider);

                //this.CurrentGame.TryLoadSaveGame();
                
                break;
            }
        }
    }

    public G? GetCurrentGame<G>(string? gameMode) where G : class
    {
        if (this.CurrentGame == null)
        {
            return null;
        }
        
        if (gameMode != null && this.CurrentGame.GameTemplate.Config.GameMode != gameMode)
        {
            throw new InvalidCastException("Requested game mode does not correspond to current game");
        }
        
        return (G)this.CurrentGame;
    }
}