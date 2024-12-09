using JetLagBRBot.Commands;
using JetLagBRBot.Game;
using JetLagBRBot.Game.Modes.BattleRoyale;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace JetLagBRBot.Services;

public interface IGameManagerService
{
    public void InitNewGame(Guid templateId, long tgGroupId);

    public G? GetCurrentGame<G>(string gameMode) where G : class;
}

public class GameManagerService : IGameManagerService
{
    private IBaseGame CurrentGame { get; set; }
    
    private readonly IGameTemplateService _gameTemplateService;
    
    private readonly IServiceProvider _serviceProvider;
    
    public GameManagerService(ICommandService commandService, ITelegramBotService telegramBotService, IGameTemplateService gameTemplateService, IServiceProvider serviceProvider)
    {
        this._gameTemplateService = gameTemplateService;
        this._serviceProvider = serviceProvider;
        
        commandService.AddCommand<NewGameCommand>();
        commandService.AddCommand<ReloadTemplatesCommand>();
        commandService.AddCommand<JoinCommandBase>();
        commandService.AddCommand<LeaveCommand>();
        commandService.AddCommand<GameStartCommand>();
        commandService.AddCommand<StopCommand>();
        commandService.AddCommand<ResetCommand>();
        telegramBotService.UpdateCommands();
    }

    public void InitNewGame(Guid templateId, long tgGroupId)
    {
        var template = this._gameTemplateService.GetGameTemplate(templateId);

        switch (template.Config.GameMode)
        {
            case BattleRoyaleGamemode.GameModeName:
            {
                var data = this._gameTemplateService.LoadGameData<BattleRoyaleGameData>(templateId);
                this.CurrentGame = new BattleRoyaleGamemode(template, data, tgGroupId, this._serviceProvider);
                break;
            }
        }
    }

    public G? GetCurrentGame<G>(string? gameMode) where G : class
    {
        if (gameMode != null && this.CurrentGame.GameTemplate.Config.GameMode != gameMode)
        {
            throw new InvalidCastException("Requested game mode does not correspond to current game");
        }
        
        return this.CurrentGame as G;
    }
}