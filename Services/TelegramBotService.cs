using System.Windows.Input;
using JetLagBRBot.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Services;

public interface ITelegramBotService
{
    public TelegramBotClient Client { get; }
    public event EventHandler<Message> OnUserLocation;
    public Task UpdateCommands();
}

public class TelegramBotService : ITelegramBotService
{
    public TelegramBotClient Client { get; private set; }
    public event EventHandler<Message> OnUserLocation; 
    
    private readonly ICommandService _commandService;
    
    public TelegramBotService(IConfiguration config, ICommandService commandService)
    {
        this._commandService = commandService;
        
        string? token = config.GetValue<string>("TG_API_KEY");

        if (string.IsNullOrEmpty(token))
        {
            throw new NullReferenceException("Telegram API Key missing");
        }
        
        this.Client = new TelegramBotClient(token);

        User user = this.Client.GetMeAsync().Result;
        
        Console.WriteLine("Telegram bot loaded");
        Console.WriteLine("id: " + user.Id);
        
        this.Client.OnMessage += this.OnMessage;
        this.Client.OnUpdate += this.OnUpdate;

        this.Client.OnError += this.OnError;
    }

    private Task OnError(Exception e, HandleErrorSource h)
    {
        Console.WriteLine(e.Message);
        return Task.CompletedTask;
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
            bool commandExists = await this._commandService.HandleCommand(msg, type);

            if (!commandExists)
            {
                await this.Client.SendTextMessageAsync(msg.Chat.Id, "Command not found");
            }
        }
    }

    private async Task OnUpdate(Update update)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            await this._commandService.HandleCallbackQuery(update);
            this.Client.AnswerCallbackQuery(update.CallbackQuery.Id);
        }
    }

    /// <summary>
    /// Gets all commands from command service and sets them in the bot
    /// </summary>
    public async Task UpdateCommands()
    {
        await this.Client.DeleteMyCommands();
        await this.Client.SetMyCommands(this._commandService.GetBotCommands());
    }
}