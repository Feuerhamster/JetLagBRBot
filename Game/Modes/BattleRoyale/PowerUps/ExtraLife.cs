namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class ExtraLifePowerup : IPowerUp
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public ExtraLifePowerup()
    {
        
    }
    
    public Task OnActivate()
    {
        throw new NotImplementedException();
    }

    public Task OnDispose()
    {
        throw new NotImplementedException();
    }

    public bool IsActive { get; set; }
}