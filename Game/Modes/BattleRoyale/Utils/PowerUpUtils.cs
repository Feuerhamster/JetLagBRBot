using JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;
using JetLagBRBot.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale.Utils;

public enum EPowerUp
{
    Immunity,
    DeactivateTracker,
    PowerUpLock,
    Stop,
    DoubleDamage,
    InstantHit,
    Vampirism,
    ExtraLife,
    Stealing,
    Clairvoyance
}


public static class PowerUpUtils
{
    private static WeightedItem<EPowerUp>[] WeightedPowerUps = [
        new(EPowerUp.Immunity, 100),
        new(EPowerUp.DeactivateTracker, 150),
        new(EPowerUp.PowerUpLock, 100),
        new(EPowerUp.Stop, 150),
        new(EPowerUp.DoubleDamage, 100),
        new(EPowerUp.InstantHit, 100),
        new(EPowerUp.Vampirism, 150),
        new(EPowerUp.ExtraLife, 100),
        new(EPowerUp.Stealing, 150),
        new(EPowerUp.Clairvoyance, 150),
    ];

    
    public static BasePowerUp GetPowerUpInstance(EPowerUp powerUp, BattleRoyaleGamemode battleRoyale, Guid ownerId)
    {
        switch (powerUp)
        {
            case EPowerUp.Immunity:
            {
                return new Immunity(battleRoyale, ownerId);
            }
            case EPowerUp.DeactivateTracker:
            {
                return new DeactivateTracker(battleRoyale, ownerId);
            }
            case EPowerUp.PowerUpLock:
            {
                return new PowerUpLock(battleRoyale, ownerId);
            }
            case EPowerUp.Stop:
            {
                return new Stop(battleRoyale, ownerId);
            }
            case EPowerUp.DoubleDamage:
            {
                return new DoubleDamage(battleRoyale, ownerId);
            }
            case EPowerUp.InstantHit:
            {
                return new InstantHit(battleRoyale, ownerId);
            }
            case EPowerUp.Vampirism:
            {
                return new Vampirism(battleRoyale, ownerId);
            }
            case EPowerUp.ExtraLife:
            {
                return new ExtraLife(battleRoyale, ownerId);
            }
            case EPowerUp.Stealing:
            {
                return new Stealing(battleRoyale, ownerId);
            }
            case EPowerUp.Clairvoyance:
            {
                return new Clairvoyance(battleRoyale, ownerId);
            }
            default:
            {
                throw new Exception("impossible powerup requested");
            }
        }
    }

    public static EPowerUp RandomPowerUp()
    {
        // TODO: Fix and improve random selection algorithm (currently not really balancend: higher weigted power ups are too often)
        var selected = WeightedRandomSelector.SelectRandomItem(WeightedPowerUps);
        return selected.Item;
    } 
}

