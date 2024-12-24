using JetLagBRBot.Game;
using JetLagBRBot.Services;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(c => c.TimestampFormat = "[HH:mm:ss] ");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();
builder.Services.AddSingleton<IGameTemplateService, GameTemplateService>();
builder.Services.AddSingleton<IGameManagerService, GameManagerService>();
builder.Services.AddSingleton<ICommandService, CommandService>();
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

var app = builder.Build();

// init game templates
IGameTemplateService gameTemplateService = app.Services.GetService<IGameTemplateService>();
var templateLog = gameTemplateService.ReloadTemplates();
app.Logger.LogInformation(String.Join("\n", templateLog.ToArray()));

app.Services.GetService<ICommandService>();

// init telegram bot
app.Services.GetService<ITelegramBotService>();

// init game manager
IGameManagerService gameManagerService = app.Services.GetService<IGameManagerService>();

gameManagerService.LoadCommands();

app.Services.GetService<IDatabaseService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();