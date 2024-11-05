using System.Windows.Input;
using JetLagBRBot.Models;
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
    
    private readonly List<BotCommand> Commands = new();

    public event EventHandler<Message> OnUserLocation; 
    
    public event EventHandler<KeyValuePair<Message, BotCommand>> OnCommand;

    public event EventHandler<KeyValuePair<Message, Document>> OnDocument;
    
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
            string rawCmd = msg.Text.Substring(1);

            var cmd = this.Commands.Find(x => x.Command == rawCmd);
            
            if (cmd == null)
            {
                await this._client.SendTextMessageAsync(msg.Chat.Id, "Command not found");
            }
            
            this.OnCommand.Invoke(this, new KeyValuePair<Message, BotCommand>(msg, cmd));
        }
    }

    private Task OnUpdate(Update update)
    {
        return Task.CompletedTask;
    }

    private void UpdateCommands()
    {
        this._client.DeleteMyCommands();
        this._client.SetMyCommands(this.Commands);
    }
    
    /// <summary>
    /// Add a single command to the already existing command list
    /// </summary>
    /// <param name="cmd">command</param>
    public void AddCommand(BotCommand cmd)
    {
        this.Commands.Add(cmd);
    }

    /// <summary>
    /// Clears the current command list and add new ones
    /// </summary>
    /// <param name="commands">List of new commands</param>
    public void SetCommands(List<BotCommand> commands)
    {
        this.Commands.Clear();
        foreach (var cmd in commands)
        {
            this.Commands.Add(cmd);
        }
    }
}