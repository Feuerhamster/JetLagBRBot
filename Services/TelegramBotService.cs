using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Services;

public interface ITelegramBotService
{
    public event EventHandler<Message> OnUserLocation; 
}

public class TelegramBotService : ITelegramBotService
{
    private readonly TelegramBotClient _client;

    public event EventHandler<Message> OnUserLocation; 
    
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
    }
    
    private Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Type == MessageType.Location)
        {
            this.OnUserLocation.Invoke(this, msg);
        }
        
        return Task.CompletedTask;
    }

    private Task OnUpdate(Update update)
    {
        return Task.CompletedTask;
    }
}