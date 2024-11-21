using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;
using Microsoft.EntityFrameworkCore;

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
            await DeferAsync(ephemeral: true);
            string respond = _cookie.GetCookie(Context.User.Id);
            await FollowupAsync($"**\U0001f960 | Twoja wróżba z chińskiego ciasteczka!** <@{Context.User.Id}>\r\n\r\n> „*{respond}*”", ephemeral: true);
        }
        else
        {
            await DeferAsync();
            string respond = _cookie.GetCookie(Context.User.Id);
            _cookie.SaveGuild(Context.User.Id, Context.Guild.Id);
            await FollowupAsync($"**\U0001f960 | Twoja wróżba z chińskiego ciasteczka!** <@{Context.User.Id}>\r\n\r\n> „*{respond}*”");
        } 
    }

    [SlashCommand("tarot", "Tarot")]
    public async Task TarotCommand()
    {
        if (_tarot.HasUserUsedTarot(Context.User.Id, Context.Guild.Id))
        {
            await DeferAsync(ephemeral: true);
            TarotCard respond = _tarot.GetTarotCard(Context.User.Id);
            await FollowupWithFileAsync(filePath: $"{tarotPath}/{respond.Name}.png", text: $"**🃏 | Twoja karta tarota na dziś!** <@{Context.User.Id}>\r\n\r\n🎴 **Wylosowana karta:** **„{respond.Name}”**  \r\n> „*{respond.Description}*”", ephemeral: true);
        }
        else
        {
            await DeferAsync();
            TarotCard respond = _tarot.GetTarotCard(Context.User.Id);
            _tarot.SaveGuild(Context.User.Id, Context.Guild.Id);
            await FollowupWithFileAsync(filePath: $"{tarotPath}/{respond.Name}.png", text: $"**🃏 | Twoja karta tarota na dziś!** <@{Context.User.Id}>\r\n\r\n🎴 **Wylosowana karta:** **„{respond.Name}”**  \r\n> „*{respond.Description}*”");
        }
    }

    [RequireTeam]
    [SlashCommand("admin", "Do not touch!")]
    public async Task AdminCommands(string command)
    {
        await DeferAsync(ephemeral: true);
        if (command == "tarot")
        {
            _db.Database.ExecuteSqlRawAsync("UPDATE Users SET Tarot = NULL, TarotGuildIds = '[]'");
            _db.SaveChangesAsync();
            await FollowupAsync("Tarot cleared!", ephemeral: true);
        }
        else if (command == "cookie")
        {
            _db.Database.ExecuteSqlRawAsync("UPDATE Users SET Cookie = NULL, CookieGuildIds = '[]'");
            _db.SaveChangesAsync();
            await FollowupAsync("Cookie cleared!", ephemeral: true);
        }
        else
        {
            await FollowupAsync("Command not recognized!", ephemeral: true);
        }
    }

}