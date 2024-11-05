using JetLagBRBot.Game;
using JetLagBRBot.Services;

namespace JetLagBRBot.GameModes.BattleRoyale;


public class BattleRoyaleGamemode : BaseGame<GameStateData, PlayerOrTeamStateData, PlayerOrTeamStateData>
{
    private readonly ITelegramBotService _telegramBot;
    
    public BattleRoyaleGamemode(ITelegramBotService tg, IServiceProvider services) : base(services)
    {
        this._telegramBot = tg;
    }
}