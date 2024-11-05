namespace JetLagBRBot.Models;

public class GameTemplate(string gameMode, string name, string filePath)
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string GameMode { get; set; } = gameMode;
    public string Name { get; set; } = name;
    public string FilePath { get; set; } = filePath;
}

public class GameTemplateConfigFile
{
    public string GameMode { get; set; }
    public string Name { get; set; }
}