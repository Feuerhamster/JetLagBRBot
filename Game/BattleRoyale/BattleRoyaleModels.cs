namespace JetLagBRBot.GameModes.BattleRoyale;


public class GameStateData
{
    public List<Landmark> Landmarks;
}

public enum EPowerUpType
{
    SelfStatus,
    Instant,
    TargetStatus
}

public interface IPowerUp
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public void Use(IServiceProvider serviceProvider);
    public EPowerUpType Type { get; set; }
}

public class PlayerOrTeamStateData
{
    public int HealthPoints { get; set; }
    public List<IPowerUp> Powerups { get; set; }
    public int TagsCount { get; set; }
}

public class Landmark(double latitude, double longitude, string name, string district)
{
    public Guid Id { get; set; } = new Guid();
    public double Latitude { get; set; } = latitude;
    public double Longitude { get; set; } = longitude;
    public string Name { get; set; } = name;
    public string District { get; set; } = district;
}