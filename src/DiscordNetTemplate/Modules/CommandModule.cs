using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;

namespace DiscordNetTemplate.Modules;

public class CommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<CommandModule> _logger;
    private readonly DatabaseBotContext _db;
    private readonly CookieModule _cookie;
    private readonly TarotModule _tarot;
    private readonly string tarotPath = "data/tarotphotos";

    public CommandModule(ILogger<CommandModule> logger, DatabaseBotContext db, CookieModule cookie, TarotModule tarot)
    {
        _logger = logger;
        _db = db;
        _cookie = cookie;
        _tarot = tarot;
    }

    [SlashCommand("cookie", "Cookie")]
    public async Task CookieCommand()
    {
        
        if (_cookie.HasUserUsedCookie(Context.User.Id, Context.Guild.Id))
        {
            string respond = _cookie.GetCookie(Context.User.Id);
            await RespondAsync($"**\U0001f960 | Twoja wróżba z chińskiego ciasteczka!**\r\n\r\n> „*{respond}*”", ephemeral: true);
        }
        else
        {
            string respond = _cookie.GetCookie(Context.User.Id);
            _cookie.SaveGuild(Context.User.Id, Context.Guild.Id);
            await RespondAsync($"**\U0001f960 | Twoja wróżba z chińskiego ciasteczka!**\r\n\r\n> „*{respond}*”");
        } 
    }

    [SlashCommand("tarot", "Tarot")]
    public async Task TarotCommand()
    {
        if (_tarot.HasUserUsedTarot(Context.User.Id, Context.Guild.Id))
        {
            TarotCard respond = _tarot.GetTarotCard(Context.User.Id);
            await RespondWithFileAsync(filePath: $"{tarotPath}/{respond.Name}.png", text: $"**🃏 | Twoja karta tarota na dziś!**\r\n\r\n🎴 **Wylosowana karta:** **„{respond.Name}”**  \r\n> „*{respond.Description}*”", ephemeral: true);
        }
        else
        {
            TarotCard respond = _tarot.GetTarotCard(Context.User.Id);
            _tarot.SaveGuild(Context.User.Id, Context.Guild.Id);
            await RespondWithFileAsync(filePath: $"{tarotPath}/{respond.Name}.png", text: $"**🃏 | Twoja karta tarota na dziś!**\r\n\r\n🎴 **Wylosowana karta:** **„{respond.Name}”**  \r\n> „*{respond.Description}*”");
        }
    }
        

    [SlashCommand("config", "Config")]
    public async Task ConfigCommand()
        => await RespondAsync("Config command");

    [SlashCommand("test", "test")]
    public async Task TestCommand()
    {
        await RespondAsync($"Saved!");
    }

}