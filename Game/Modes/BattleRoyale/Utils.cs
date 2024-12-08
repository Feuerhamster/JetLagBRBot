using JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

namespace JetLagBRBot.Game.Modes.BattleRoyale;

public enum EPowerUp
{
    ExtraLife
}

public static class PowerUpUtils
{
    public static IPowerUp? GetPowerUp(EPowerUp powerUp, BattleRoyaleGamemode battleRoyale, IServiceProvider serviceProvider)
    {
        switch (powerUp)
        {
            case EPowerUp.ExtraLife:
            {
                return ActivatorUtilities.CreateInstance<ExtraLifePowerup>(serviceProvider);
            }
            default:
            {
                return null;
            }
        }
    }

    public static EPowerUp RandomPowerUp()
    {
        var values = Enum.GetValues(typeof(EPowerUp));
        var random = new Random();
        return (EPowerUp)values.GetValue(random.Next(values.Length));
    } 
}