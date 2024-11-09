using JetLagBRBot.Commands;
using JetLagBRBot.Game;
using JetLagBRBot.GameModes.BattleRoyale;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace JetLagBRBot.Services;

public interface IGameManagerService
{
    public void InitNewGame(Guid templateId, int tgGroupId);
}

public class GameManagerService : IGameManagerService
{
    public IBaseGame? CurrentGame { get; set; }
    
    private readonly IGameTemplateService _gameTemplateService;
    
    private readonly IServiceProvider _serviceProvider;
    
    public GameManagerService(ICommandService commandService, ITelegramBotService telegramBotService, IGameTemplateService gameTemplateService, IServiceProvider serviceProvider)
    {
        this._gameTemplateService = gameTemplateService;
        this._serviceProvider = serviceProvider;
        
        commandService.AddCommand<NewGameCommand>();
        commandService.AddCommand<ReloadTemplatesCommand>();
        telegramBotService.UpdateCommands();
    }

    public void InitNewGame(Guid templateId, int tgGroupId)
    {
        var t = this._gameTemplateService.GetGameTemplate(templateId);
        var d = this._gameTemplateService.LoadGameData<BattleRoyaleGameData>(templateId);

        switch (t.Config.GameMode)
        {
            case "BattleRoyale":
            {
                this.CurrentGame = new BattleRoyaleGamemode(t, d, this._serviceProvider);
                break;
            }
        }
    }
}