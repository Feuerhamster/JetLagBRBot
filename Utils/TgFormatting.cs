namespace JetLagBRBot.Utils;

public static class TgFormatting
{
    public static string UserMention(long userId, string displayName)
    {
        return $"[{displayName}](tg://user?id={userId}) ";
    }
}