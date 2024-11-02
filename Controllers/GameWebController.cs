using Microsoft.AspNetCore.Mvc;

namespace JetLagBRBot.Controllers;

[ApiController]
[Route("/")]
public class GameWebController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<GameWebController> _logger;

    public GameWebController(ILogger<GameWebController> logger)
    {
        _logger = logger;
    }
}