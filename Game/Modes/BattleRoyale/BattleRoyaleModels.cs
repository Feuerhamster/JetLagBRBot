using System.Text.Json.Serialization;
using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale;


public class GameStateData
{
    public List<Landmark> Landmarks { get; set; } = new();

    /// <summary>
    /// Key is the tagger and value the victim
    /// </summary>
    public List<PlayerTag> PlayerTags { get; set; } = new();

    public DateTime LastTimeDropped { get; set; } = DateTime.MinValue;
    
    public Landmark? CurrentActiveLandmark { get; set; } = null;
}

public class PlayerTag
{
    [JsonConstructor]
    private PlayerTag()
    {
        
    }

    public PlayerTag(Guid taggerId, Guid victimId, int damage = 1)
    {
        TaggerId = taggerId;
        VictimId = victimId;
        Damage = damage;
    }
    
    public Guid TagId { get; private set; } = Guid.NewGuid();
    public DateTime TagTime { get; set; } = DateTime.Now;
    public Guid TaggerId { get; set; }
    public Guid VictimId { get; set; }
    public int Damage { get; set; }
}

public enum EPlayerTagStatus
{
    Default,
    Frozen,
    Protected
}

public class PlayerOrTeamStateData
{
    public int HealthPoints { get; set; } = 3;
    public List<BasePowerUp> Powerups { get; set; } = new();
    public EPlayerTagStatus TagStatus { get; set; } = EPlayerTagStatus.Default;
}

public class Landmark
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public float[] Coordinates { get; set; }
    public string Name { get; set; }
    public string District { get; set; }
    public string ImagePath { get; set; }
    
    public bool Claimed { get; set; } = false;
}

public class BattleRoyaleGameData
{
    public TimeSpan TimeBetweenDrops { get; set; }
    public List<Landmark> Landmarks { get; set; }
    public TimeSpan TagFreeze { get; set; }
    public TimeSpan AfterTagProtection { get; set; }
}

public class TagEventArgs(PlayerTag tag) : EventArgs
{
    public PlayerTag PlayerTag { get; set; } = tag;
    public bool Cancel { get; set; } = false;
}

public class SuccessfulTagEventArgs(PlayerTag tag) : EventArgs
{
    public readonly PlayerTag PlayerTag = tag;
}

public class PowerUpUseEventArgs(BasePowerUp powerUp, Guid playerId) : EventArgs
{
    public BasePowerUp PowerUp { get; private set; } = powerUp;
    
    public Guid PlayerId { get; set; } = playerId;
    public bool Cancel { get; set; } = false;
}