namespace JetLagBRBot.Game.Modes.BattleRoyale.PowerUps;

public class ExtraLifePowerup : IPowerUp
{
    public EPowerUp PowerUp { get; private set; }
    public bool IsActive { get; private set; }

    public ExtraLifePowerup(BattleRoyaleGamemode game, IServiceProvider services)
    {
        
    }
    
    public Task OnActivate()
    {
        this.IsActive = true;
        return Task.CompletedTask;
    }

    public Task OnDispose()
    {
        this.IsActive = false;
        return Task.CompletedTask;
    }
}