namespace JetLagBRBot.Utils;

public static class TgFormatting
{
    public static string UserMention(long userId, string displayName)
    {
        return $"[{displayName}](tg://user?id={userId})";
    }

    /// <summary>
    /// https://core.telegram.org/bots/api#markdownv2-style
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string MarkdownEscape(string input)
    {
        return input
            .Replace("!", @"\!")
            .Replace("-", @"\-")
            .Replace(".", @"\.")
            .Replace("+", @"\+")
            .Replace("(", @"\(")
            .Replace(")", @"\)");
    }

    public static string EncodeCallbackPayloadData(string[] args)
    {
        return string.Join("|", args);
    }

    public static string[] DecodeCallbackPayloadData(string payload)
    {
        return payload.Split('|');
    }
}