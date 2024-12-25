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
    public static WeightedItem<EPowerUp>[] WeightedPowerUps = [
        new(EPowerUp.Immunity, 100),
        new(EPowerUp.DeactivateTracker, 120),
        new(EPowerUp.PowerUpLock, 100),
        new(EPowerUp.Stop, 120),
        new(EPowerUp.DoubleDamage, 100),
        new(EPowerUp.InstantHit, 100),
        new(EPowerUp.Vampirism, 120),
        new(EPowerUp.ExtraLife, 100),
        new(EPowerUp.Stealing, 120),
        new(EPowerUp.Clairvoyance, 120),
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
}

