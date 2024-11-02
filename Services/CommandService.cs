using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Services;

public interface ICommand
{
    public string Command { get; }
    public string Description { get; }
    public Task Execute(Message msg, UpdateType type);
}

public interface ICommandService
{
    public List<BotCommand> GetBotCommands();
    public void AddCommand(ICommand command);
    public Task<bool> HandleCommand(string command, Message msg, UpdateType type);
}

public class CommandService: ICommandService
{
    private readonly Dictionary<string, ICommand> Commands = new();

    public List<BotCommand> GetBotCommands()
    {
        List<BotCommand> botCommands = [];

        foreach (var Command in this.Commands)
        {
            botCommands.Add(new BotCommand()
            {
                Command = "/" + Command.Key,
                Description = Command.Value.Description
            });
        }

        return botCommands;
    }

    public void AddCommand(ICommand cmd)
    {
        this.Commands.Add(cmd.Command, cmd);
    }

    public async Task<bool> HandleCommand(string command, Message msg, UpdateType type)
    {
        if (!this.Commands.ContainsKey(command))
        {
            return false;
        }
        
        this.Commands[command].Execute(msg, type);
        return true;
    }
}