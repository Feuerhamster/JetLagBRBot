using System.Security.Cryptography.X509Certificates;

namespace JetLagBRBot.Models;

public class GameTemplate(GameTemplateConfigFile config, string filePath)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public GameTemplateConfigFile Config { get; set; } = config;
    public string FilePath { get; set; } = filePath;
}

public class GameTemplateConfigFile
{
    public string GameMode { get; set; }
    public string Name { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan? ProtectionPhase { get; set; }
}