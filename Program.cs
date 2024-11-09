using JetLagBRBot.Game;
using JetLagBRBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();
builder.Services.AddSingleton<IGameTemplateService, GameTemplateService>();
builder.Services.AddSingleton<IGameLogService, GameLogService>();
builder.Services.AddSingleton<IGameManagerService, GameManagerService>();
builder.Services.AddSingleton<ICommandService, CommandService>();

var app = builder.Build();

// init game templates
IGameTemplateService gameTemplateService = app.Services.GetService<IGameTemplateService>();
var templateLog = gameTemplateService.ReloadTemplates();
Console.WriteLine(String.Join("\n", templateLog.ToArray()));

// init game manager
app.Services.GetService<IGameManagerService>();

// init telegram bot
app.Services.GetService<ITelegramBotService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();