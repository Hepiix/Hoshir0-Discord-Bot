using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;
using DiscordNetTemplate.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordNetTemplate.Services;

public class DiscordBotService : BackgroundService
{
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IConfiguration _config;
        private readonly ILogger<DiscordBotService> _logger;
        private readonly InteractionHandler _interactionHandler;
        private readonly IServiceProvider _serviceProvider;
    public DiscordBotService(
                DiscordSocketClient client,
                InteractionService interactions,
                IConfiguration config,
                ILogger<DiscordBotService> logger,
                InteractionHandler interactionHandler,
                IServiceProvider serviceProvider)
            {
                _client = client;
                _interactions = interactions;
                _config = config;
                _logger = logger;
                _interactionHandler = interactionHandler;
                _serviceProvider = serviceProvider;
            }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Ready += ClientReady;
        await _client.SetGameAsync("Las Vegas 🎰 /config");
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceviedAsync;
        _client.JoinedGuild += OnGuildJoin;
        _interactions.Log += LogAsync;

        await _interactionHandler.InitializeAsync();

        using var scope = _serviceProvider.CreateScope();
        var timerTasks = scope.ServiceProvider.GetRequiredService<TimerTasks>();
        timerTasks.StartTasks();

        await _client.LoginAsync(TokenType.Bot, _config["Secrets:Discord"]);
        await _client.StartAsync();

        // Możesz tu dodać pętlę jeśli chcesz np. cykliczne zadania,
        // albo po prostu czekać na zatrzymanie usługi:
        await Task.Delay(-1, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task ClientReady()
    {
        _logger.LogInformation("Logged as {User}", _client.CurrentUser);

        await _interactions.RegisterCommandsGloballyAsync();
    }

    public Task MessageAsync(SocketMessage message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
    public Task LogAsync(LogMessage msg)
    {
        var severity = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        };

        _logger.Log(severity, msg.Exception, msg.Message);

        return Task.CompletedTask;
    }

    private async Task MessageReceviedAsync(SocketMessage message)
    {
        if (message.Author.Id != _client.CurrentUser.Id)
            _logger.LogInformation(message.Content);
    }

    private async Task OnGuildJoin(SocketGuild guild)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseBotContext>();

        var existing = await db.GuildConfigs.FirstOrDefaultAsync(g => g.Id == guild.Id);
        if (existing == null)
        {
            GuildConfigModel guildModel = new()
            {
                Id = guild.Id,
                AnimeInfoChannelId = null,
                GamblingChannel = null
            };
            db.GuildConfigs.Add(guildModel);
            await db.SaveChangesAsync();
            _logger.LogInformation($"Added to database!");
        }
        else
        {
            _logger.LogInformation("Already in database!");
        }
    }
}