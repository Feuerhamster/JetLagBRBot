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
    
    private readonly ICommandService _commandService;

    public event EventHandler<Message> OnUserLocation; 
    
    public TelegramBotService(IConfiguration config, ICommandService commandService)
    {
        this._commandService = commandService;
        
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

        this._client.DeleteMyCommandsAsync().Wait();
        List<BotCommand> commands = this._commandService.GetBotCommands();
        this._client.SetMyCommandsAsync(commands);
    }
    
    private async Task OnMessage(Message msg, UpdateType type)
    {
        // is a location update
        if (msg.Type == MessageType.Location)
        {
            this.OnUserLocation.Invoke(this, msg);
        }
        // Is a command
        else if (msg.Type == MessageType.Text && msg.Text.StartsWith("/"))
        {
            bool commandExists = await this._commandService.HandleCommand(msg.Text.Substring(1), msg, type);

            if (!commandExists)
            {
                await this._client.SendTextMessageAsync(msg.Chat.Id, "Command not found");
            }
        }
    }

    private Task OnUpdate(Update update)
    {
        return Task.CompletedTask;
    }
}