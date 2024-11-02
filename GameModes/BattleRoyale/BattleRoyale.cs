using JetLagBRBot.Services;

namespace JetLagBRBot.GameModes.BattleRoyale;

public class BattleRoyaleGamemode : BaseGame
{
    private readonly ITelegramBotService _telegramBot;
    
    public BattleRoyaleGamemode(ITelegramBotService tg)
    {
        this._telegramBot = tg;
    }
}