using JetLagBRBot.Game;
using JetLagBRBot.Models;
using JetLagBRBot.Services;

namespace JetLagBRBot.GameModes.BattleRoyale;


public class BattleRoyaleGamemode : BaseGame<GameStateData, PlayerOrTeamStateData, PlayerOrTeamStateData>
{
    public BattleRoyaleGamemode(GameTemplate template, BattleRoyaleGameData data, IServiceProvider services) : base(template, services)
    {

    }
}