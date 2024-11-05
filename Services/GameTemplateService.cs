using System.Text.Json;
using JetLagBRBot.Models;

namespace JetLagBRBot.Services;

public interface IGameTemplateService
{
    public List<string> ReloadTemplates();
}

public class GameTemplateService : IGameTemplateService
{
    private const string CONFIG_FILE = "config.json";
    private const string GAMEDATA_FILE = "gamedata.json";
        
    private List<GameTemplate> GameTemplates { get; set; } = new();
    
    public GameTemplateService()
    {
    }

    public List<string>  ReloadTemplates()
    {
        List<string> log = new();
        
        var directories = Directory.GetDirectories($"{AppContext.BaseDirectory}/Game/Templates");
        foreach (var dir in directories)
        {
            string configPath = Path.Combine(dir, CONFIG_FILE);
            
            if (!File.Exists(configPath))
            {
                log.Add("GameMode could not be loaded (config.json not found)");
                log.Add(configPath);
                log.Add("");
                continue;
            }
            
            string rawConfig = File.ReadAllText(configPath);

            GameTemplateConfigFile config;
            
            try
            {
                config = JsonSerializer.Deserialize<GameTemplateConfigFile>(rawConfig);
            }
            catch (JsonException e)
            {
                log.Add("Invalid config file");
                log.Add(configPath);
                log.Add(e.Message);
                log.Add("");
                continue;
            }
            catch (Exception e)
            {
                log.Add("Unknown error parsing config");
                log.Add(configPath);
                log.Add(e.Message);
                log.Add("");
                continue;
            }

            var t = new GameTemplate(config.GameMode, config.Name, dir);
            
            this.GameTemplates.Add(t);

            log.Add($"Successfully loaded game template: {t.Name}");
        }

        return log;
    }

    public D? LoadGameData<D>(string gameModeName)
    {
        GameTemplate gameTemplate = this.GameTemplates.FirstOrDefault(x => x.Name == gameModeName);

        if (gameTemplate == null) throw new Exception("Game data not found for " + gameModeName);

        string rawConfig = File.ReadAllText(Path.Combine(gameTemplate.FilePath, GAMEDATA_FILE));
            
        var gamedata = JsonSerializer.Deserialize<D>(rawConfig);
        
        return gamedata;
    }
}