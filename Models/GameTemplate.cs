namespace JetLagBRBot.Models;

public class GameTemplate(GameTemplateConfigFile config, string filePath)
{
    public Guid id { get; private set; } = Guid.NewGuid();
    public GameTemplateConfigFile Config { get; set; } = config;
    public string FilePath { get; set; } = filePath;
}

public class GameTemplateConfigFile
{
    public string GameMode { get; set; }
    public string Name { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan? ProtectionPhase { get; set; }
    public TimeSpan? TagFreeze { get; set; }
    public TimeSpan? AfterTagProtection { get; set; }
}