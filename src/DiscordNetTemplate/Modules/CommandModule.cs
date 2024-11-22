using Discord.Commands;
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
    private readonly GamblingModule _slot;
    private readonly string tarotPath = "data/tarotphotos";

    public CommandModule(ILogger<CommandModule> logger, DatabaseBotContext db, CookieModule cookie, TarotModule tarot, GamblingModule slot)
    {
        _logger = logger;
        _db = db;
        _cookie = cookie;
        _tarot = tarot;
        _slot = slot;
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
            string win = _slot.AddMoney(Context.User.Id, 50);
            await FollowupAsync($"**\U0001f960 | Twoja wróżba z chińskiego ciasteczka!** <@{Context.User.Id}>\r\n\r\n> „*{respond}*”\n{win}");
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
            string win = _slot.AddMoney(Context.User.Id, 50);
            await FollowupWithFileAsync(filePath: $"{tarotPath}/{respond.Name}.png", text: $"**🃏 | Twoja karta tarota na dziś!** <@{Context.User.Id}>\r\n\r\n🎴 **Wylosowana karta:** **„{respond.Name}”**  \r\n> „*{respond.Description}*”\n{win}");
        }
    }

    [RequireTeam]
    [SlashCommand("admin", "Do not touch!")]
    public async Task AdminCommands(string command, string userId)
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

    [SlashCommand("profile", "Profile")]
    public async Task ProfileCommand()
    {
        await DeferAsync(ephemeral:true);

        var user = _db.Users.FirstOrDefault(u => u.Id == Context.User.Id);

        if (user == null)
        {
            await FollowupAsync("Brak profilu");
        }
        else
        {
            await FollowupAsync($"{Context.User.Username}\nŻetony: {user.Money}");
        }
    }

    [SlashCommand("slot", "Slots!")]
    public async Task SlotCommand(int bet)
    {
        await DeferAsync();
        var user = _db.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (user == null || user.Money < bet || user.Money == 0)
        {
            await FollowupAsync("Nie stać cię!");
            return;
        }
        else
        {
            user.Money -= bet;
            _db.SaveChangesAsync();
        }
        

        string[] result = new string[3] { "<a:slot:1309190616755732561>", "<a:slot:1309190616755732561>", "<a:slot:1309190616755732561>" };

        string initialResult = $"🎰 {result[0]} | {result[1]} | {result[2]} <@{Context.User.Id}>";
        await ModifyOriginalResponseAsync(msg => msg.Content = initialResult);

        for (int i = 0; i < 3; i++)
        {
            result[i] = _slot.GenerateFinalRoll()[i];

            string updatedResult = $"🎰 {result[0]} | {result[1]} | {result[2]} <@{Context.User.Id}>";
            await ModifyOriginalResponseAsync(msg => msg.Content = updatedResult);

            await Task.Delay(1000);
        }

        string finalRoll = $"{result[0]} | {result[1]} | {result[2]} <@{Context.User.Id}>";

        string winMessage = _slot.CheckWin(result, bet, Context.User.Id);

        await ModifyOriginalResponseAsync(msg => msg.Content = $"🎰 {finalRoll}\n{winMessage}");
    }
}