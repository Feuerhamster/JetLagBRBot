using System.Text.Json.Serialization;
using System.Timers;

namespace JetLagBRBot.Utils;

public class ManagedTimer
{
    public event EventHandler OnStarted;
    public event EventHandler OnStopped;
    public event EventHandler OnResumed;
    public event EventHandler OnFinished;
    public event EventHandler OnTick;
    
    private readonly System.Timers.Timer _timer;

    public TimeSpan Duration { get; set; }
    public DateTime TimeStarted { get; set; }

    [JsonConstructor]
    private ManagedTimer()
    {
        
    }
    
    public ManagedTimer(TimeSpan duration, int tickDuration = 5000)
    {
        this.Duration = duration;
        this._timer = new System.Timers.Timer(tickDuration);
        this._timer.Elapsed += this.Tick;
    }

    public void Start()
    {
        this._timer.Start();

        if (this.TimeStarted == default)
        {
            this.TimeStarted = DateTime.Now;
            this.OnStarted?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            this.OnResumed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Stop()
    {
        this.OnStopped?.Invoke(this, EventArgs.Empty);
        this._timer.Stop();
    }

    public void Reset()
    {
        this._timer.Stop();
        this.TimeStarted = default;
    }

    public void Finish()
    {
        this._timer.Stop();
        this.OnFinished?.Invoke(this, EventArgs.Empty);
        this.Reset();
    }

    private void Tick(object sender, ElapsedEventArgs e)
    {
        this.OnTick?.Invoke(this, EventArgs.Empty);
        
        // check if timer has finished
        if (DateTime.Now >= this.TimeStarted.Add(this.Duration))
        {
            this.Finish();
        }
    }
}