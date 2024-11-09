using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;

namespace DiscordNetTemplate.Modules;

public class CommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<CommandModule> _logger;
    private readonly DatabaseBotContext _db;
    private readonly CookieModule _cookie;

    public CommandModule(ILogger<CommandModule> logger, DatabaseBotContext db, CookieModule cookie)
    {
        _logger = logger;
        _db = db;
        _cookie = cookie;
    }

    [SlashCommand("cookie", "Cookie")]
    public async Task CookieCommand()
    {
        string respond = _cookie.GetCookie(Context.User.Id);
        await RespondAsync(respond);
    }

    [SlashCommand("tarot", "Tarot")]
    public async Task TarotCommand()
        => await RespondAsync("Tarot Command");

    [SlashCommand("config", "Config")]
    public async Task ConfigCommand()
        => await RespondAsync("Config command");

    [SlashCommand("test", "test")]
    public async Task TestCommand()
    {
        await RespondAsync($"Saved!");
    }

}