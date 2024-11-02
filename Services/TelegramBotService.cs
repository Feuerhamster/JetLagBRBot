using JetLagBRBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Services;

public interface ITelegramBotService
{ 
    public event EventHandler<UserLocationEventArgs> OnUserLocation; 
}

public class TelegramBotService : ITelegramBotService
{
    private TelegramBotClient _client;

    public event EventHandler<UserLocationEventArgs> OnUserLocation; 
    
    public TelegramBotService(IConfiguration config)
    {
        string? token = config.GetValue<string>("TG_API_KEY");

        if (string.IsNullOrEmpty(token))
        {
            throw new NullReferenceException("Telegram API Key missing");
        }
        
        this._client = new TelegramBotClient(token);

        User user = this._client.GetMeAsync().Result;
        
        Console.WriteLine("Telegram bot loaded");
        Console.WriteLine("id: " + user.Id);
        
        this._client.OnMessage += this.OnMessage;
        this._client.OnUpdate += this.OnUpdate;
        
        this.OnUserLocation += this.Locate;
    }

    private void Locate(object sender, UserLocationEventArgs args)
    {
        Console.WriteLine($"{args.Location.Latitude} / {args.Location.Longitude}");
    }
    
    private Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Type == MessageType.Location)
        {
            this.OnUserLocation.Invoke(this, new UserLocationEventArgs() { Location = msg.Location });
        }
        
        return Task.CompletedTask;
    }

    private Task OnUpdate(Update update)
    {
        return Task.CompletedTask;
    }
}