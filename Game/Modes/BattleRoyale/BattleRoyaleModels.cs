namespace JetLagBRBot.GameModes.BattleRoyale;


public class GameStateData
{
    public List<Landmark> Landmarks;

    /// <summary>
    /// Key is the tagger and value the victim
    /// </summary>
    public List<KeyValuePair<Guid, Guid>> PlayerTags;

    public DateTime LastTimeDropped;
    
    public Landmark CurrentActiveLandmark;
}

public enum EPowerupActivator
{
    OnTag,
    OnTagged,
    Instant,
}

public interface IPowerUp
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Task<bool> Use(BattleRoyaleGamemode gamemode, IServiceProvider serviceProvider);
    public EPowerupActivator Activator { get; set; }
    public bool IsActive { get; set; }
}

public class PlayerOrTeamStateData
{
    public int HealthPoints { get; set; }
    public List<IPowerUp> Powerups { get; set; }
}

public class Landmark(double latitude, double longitude, string name, string district, string image)
{
    public Guid Id { get; set; } = new Guid();
    public double[] Coordinates { get; set; } = [latitude, longitude];
    public string Name { get; set; } = name;
    public string District { get; set; } = district;
    public string Image { get; set; } = image;
}

public class BattleRoyaleGameData
{
    public TimeSpan TimeBetweenDrops { get; set; }
    public List<Landmark> Landmarks { get; set; }
    public TimeSpan? TagFreeze { get; set; }
    public TimeSpan? AfterTagProtection { get; set; }
}