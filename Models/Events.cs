using Telegram.Bot.Types;

namespace JetLagBRBot.Models;

public class UserLocationEventArgs : EventArgs
{
    public Location Location { get; set; }
}
