using System.Windows.Input;
using JetLagBRBot.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Services;



public interface ICommandService
{
    public List<BotCommand> GetBotCommands();
    public void AddCommand(IBotCommand command);
    public Task<bool> HandleCommand(string command, Message msg, UpdateType type);
}

/**
 * DI for non http request bound services (needed for commands)
 * https://stackoverflow.com/questions/37189984/dependency-injection-with-classes-other-than-a-controller-class
 */

public class CommandService: ICommandService
{
    private readonly Dictionary<string, IBotCommand> Commands = new();

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

    public void AddCommand(IBotCommand cmd)
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