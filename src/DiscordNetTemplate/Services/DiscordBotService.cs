using DiscordNetTemplate.Db;
using DiscordNetTemplate.Modules;
using Microsoft.Extensions.Hosting;
using DiscordNetTemplate.Models;

namespace DiscordNetTemplate.Services;

public class DiscordBotService(DiscordSocketClient client, InteractionService interactions, IConfiguration config, ILogger<DiscordBotService> logger,
    InteractionHandler interactionHandler, TimerTasks timerTasks, DatabaseBotContext db) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        client.Ready += ClientReady;
        client.SetGameAsync("Las Vegas 🎰");
        timerTasks.StartTasks();
        client.Log += LogAsync;
        client.MessageReceived += MessageReceviedAsync;
        client.JoinedGuild += OnGuildJoin;
        interactions.Log += LogAsync;

        return interactionHandler.InitializeAsync()
            .ContinueWith(t => client.LoginAsync(TokenType.Bot, config["Secrets:Discord"]), cancellationToken)
            .ContinueWith(t => client.StartAsync(), cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (ExecuteTask is null)
            return Task.CompletedTask;

        base.StopAsync(cancellationToken);
        return client.StopAsync();
    }

    private async Task ClientReady()
    {
        logger.LogInformation("Logged as {User}", client.CurrentUser);

        await interactions.RegisterCommandsGloballyAsync();
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

        logger.Log(severity, msg.Exception, msg.Message);

        return Task.CompletedTask;
    }

    private async Task MessageReceviedAsync(SocketMessage message)
    {
        if (message.Author.Id != client.CurrentUser.Id)
            logger.LogInformation(message.Content);
    }

    private async Task OnGuildJoin(SocketGuild guild)
    {
        if (db.GuildConfigs.FirstOrDefault(g => g.Id == guild.Id) == null)
        {
            GuildConfigModel guildModel = new()
            {
                Id = guild.Id,
                AnimeInfoChannelId = null,
                GamblingChannel = null
            };
            db.GuildConfigs.Add(guildModel);
            db.SaveChangesAsync();
            logger.LogInformation($"Added to database!");
        }
        else
        {
            logger.LogInformation("Already in database!");
        }
    }
}