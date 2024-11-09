using System.Text.RegularExpressions;
using Telegram.Bot.Types.ReplyMarkups;

namespace JetLagBRBot.Utils;

public class InlineKeyboardChoiceFactory(string identifier)
{
    private readonly InlineKeyboardMarkup keyboardMarkup = new();
    
    public static readonly Regex payloadRegex = new(@"\@(\w+);(.*)");

    private string FormatButtonData(string data)
    {
        return $"@{identifier};{data}";
    }

    /// <summary>
    /// For usage in telegram bot update method to extract previously formatted button data
    /// </summary>
    /// <param name="payload"></param>
    /// <returns>identifier and data or, in case of not matching, null</returns>
    public static KeyValuePair<string, string>? ExtractUpdatePayload(string payload)
    {
        Match match = InlineKeyboardChoiceFactory.payloadRegex.Match(payload);

        if (!match.Success) return null;
        
        return new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value);
    }
    
    public void AddChoice(string label, string value)
    {
        this.keyboardMarkup.AddButton(label, this.FormatButtonData(value));
        this.keyboardMarkup.AddNewRow();
    }

    public InlineKeyboardMarkup GetKeyboardMarkup()
    {
        return this.keyboardMarkup;
    }
}