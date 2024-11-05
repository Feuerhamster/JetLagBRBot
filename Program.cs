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

var app = builder.Build();

// init telegram bot
app.Services.GetService<ITelegramBotService>();

IGameTemplateService gameTemplateService = app.Services.GetService<IGameTemplateService>();

var templateLog = gameTemplateService.ReloadTemplates();

Console.WriteLine(String.Join("\n", templateLog.ToArray()));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();