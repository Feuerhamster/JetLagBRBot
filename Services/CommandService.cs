using System.Text.RegularExpressions;
using JetLagBRBot.Models;
using JetLagBRBot.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetLagBRBot.Services;

public interface ICommandService
{
    public List<BotCommand> GetBotCommands();
    public void AddCommand<C>() where C : CustomBotCommandBase;
    public Task<bool> HandleCommand(Message msg, UpdateType type);
    public Task<bool> HandleCallbackQuery(Update update);
}

public class CommandService(IServiceProvider serviceProvider) : ICommandService
{
    private readonly Dictionary<string, CustomBotCommandBase> Commands = new();

    private readonly Regex commandMatcher = new(@"/([a-z0-9_]+)");
    
    public List<BotCommand> GetBotCommands()
    {
        List<BotCommand> botCommands = [];

        // add necessary standard start command
        botCommands.Add(new BotCommand() { Command = "/start", Description = "initializes interaction with the bot" });
        
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

    /// <summary>
    /// Add command with automatic dependency injection of services
    /// </summary>
    /// <typeparam name="C"></typeparam>
    public void AddCommand<C>() where C : CustomBotCommandBase
    {
        var cmd = ActivatorUtilities.CreateInstance<C>(serviceProvider);
        this.Commands.Add(cmd.Command, cmd);
    }

    public async Task<bool> HandleCommand(Message msg, UpdateType type)
    {
        Match match = commandMatcher.Match(msg.Text);

        if (!match.Success) return false;
        
        if (!this.Commands.TryGetValue(match.Groups[1].Value, out var command))
        {
            return false;
        }

        // Handle constraints
        if (command.Constraints != null)
        {
            foreach (var constraint in command.Constraints)
            {
                var res = await constraint.Execute(serviceProvider, msg);

                if (res == false) return false;
            }   
        }
        
        await command.Execute(msg, type);
        return true;
    }

    public async Task<bool> HandleCallbackQuery(Update update)
    {
        var extractedUpdate = InlineKeyboardChoiceFactory.ExtractUpdatePayload(update.CallbackQuery.Data);

        if (extractedUpdate == null) return false;
        
        if (!this.Commands.TryGetValue(extractedUpdate.Value.Key, out var command))
        {
            return false;
        }
        
        await command.OnCallbackQuery(update, extractedUpdate.Value.Value);
        return true;
    }
}