using JetLagBRBot.Game.Modes.BattleRoyale.Utils;

namespace JetLagBRBot.Game.Modes.BattleRoyale;


public class GameStateData
{
    public List<Landmark> Landmarks = new();

    /// <summary>
    /// Key is the tagger and value the victim
    /// </summary>
    public List<PlayerTag> PlayerTags = new();

    public DateTime LastTimeDropped = DateTime.MinValue;
    
    public Landmark? CurrentActiveLandmark = null;
}

public class PlayerTag(Guid taggerId, Guid victimId, int damage = 0)
{
    public Guid TagId { get; private set; } = Guid.NewGuid();
    public DateTime TagTime { get; set; } = DateTime.Now;
    public Guid TaggerId { get; set; } = taggerId;
    public Guid VictimId { get; set; } = victimId;
    public int Damage { get; set; } = damage;
}

public class PlayerOrTeamStateData
{
    public int HealthPoints { get; set; }
    public List<BasePowerUp> Powerups { get; set; }
}

public class Landmark(double latitude, double longitude, string name, string district, string image)
{
    public Guid Id { get; set; } = new Guid();
    public double[] Coordinates { get; set; } = [latitude, longitude];
    public string Name { get; set; } = name;
    public string District { get; set; } = district;
    public string Image { get; set; } = image;
    
    public bool Claimed { get; set; } = false;
}

public class BattleRoyaleGameData
{
    public TimeSpan TimeBetweenDrops { get; set; }
    public List<Landmark> Landmarks { get; set; }
    public TimeSpan? TagFreeze { get; set; }
    public TimeSpan? AfterTagProtection { get; set; }
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