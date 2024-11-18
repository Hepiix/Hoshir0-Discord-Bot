global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
using DiscordNetTemplate.Db;
using DiscordNetTemplate.Modules;
using DiscordNetTemplate.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;


var builder = new HostApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables("DNetTemplate_");

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:yy.MM.dd_HH.mm}.log")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(loggerConfig, dispose: true);

builder.Services.AddSingleton(new DiscordSocketClient(
    new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
        FormatUsersInBidirectionalUnicode = false,
        // Add GatewayIntents.GuildMembers to the GatewayIntents and change this to true if you want to download all users on startup
        AlwaysDownloadUsers = false,
        LogGatewayIntentWarnings = false,
        LogLevel = LogSeverity.Info
    }));

builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()
{
    LogLevel = LogSeverity.Info
}));

builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddScoped<CookieModule>();
builder.Services.AddScoped<TarotModule>();
builder.Services.AddScoped<TimerService>();
builder.Services.AddScoped<TimerTasks>();  

builder.Services.AddDbContext<DatabaseBotContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddHostedService<DiscordBotService>();

var app = builder.Build();

await app.RunAsync();
