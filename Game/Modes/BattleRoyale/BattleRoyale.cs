using JetLagBRBot.Game;
using JetLagBRBot.Services;

namespace JetLagBRBot.GameModes.BattleRoyale;


public class BattleRoyaleGamemode : BaseGame<GameStateData, PlayerOrTeamStateData, PlayerOrTeamStateData>
{
    private readonly ITelegramBotService _telegramBot;
    private readonly BattleRoyaleGameData _gameData;
    
    public BattleRoyaleGamemode(ITelegramBotService tg, IGameTemplateService ts, IServiceProvider services) : base(services)
    {
        this._telegramBot = tg;
    }
}