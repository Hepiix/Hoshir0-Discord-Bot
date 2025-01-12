using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;
using DiscordNetTemplate.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace DiscordNetTemplate.Modules;

public class TimerTasks
{
    private readonly TimerService _timerService;
    private readonly ILogger _logger;
    private readonly DatabaseBotContext _botContext;
    private readonly DiscordSocketClient _client;

    public TimerTasks(TimerService timerService, ILogger<DiscordBotService> logger, DatabaseBotContext botContext, DiscordSocketClient client)
    {
        _timerService = timerService;
        _logger = logger;
        _botContext = botContext;
        _client = client;
    }

    public void StartTasks()
    {
        _logger.LogInformation("Registering tasks...");
        _timerService.RegisterDailyTask(CookieClear, 11, 00);
        _timerService.RegisterDailyTask(TarotClear, 23, 00);
        _timerService.RegisterTask(AnimeInfo, TimeSpan.FromMinutes(10));
    }

    public async Task CookieClear()
    {
        _botContext.Database.ExecuteSqlRawAsync("UPDATE Users SET Cookie = NULL, CookieGuildIds = '[]'");
        _botContext.SaveChangesAsync();
        _logger.LogInformation("Db saved");
    }

    public async Task TarotClear()
    {
        _botContext.Database.ExecuteSqlRawAsync("UPDATE Users SET Tarot = NULL, TarotGuildIds = '[]'");
        _botContext.SaveChangesAsync();
        _logger.LogInformation("Db saved");
    }

    public async Task AnimeInfo()
    {
        AnimeNews news = await GetAnimeNews("https://www.animenewsnetwork.com/all/atom.xml?ann-edition=us");

        if (news == null || (DateTime.UtcNow - news.Published).TotalMinutes >= 10)
        {
            return;
        }
        else
            SendNews(news);
    }

    private async Task<AnimeNews?> GetAnimeNews(string url)
    {
        using HttpClient client = new HttpClient();
        string atomData = await client.GetStringAsync(url);

        XDocument atomXml = XDocument.Parse(atomData);

        XNamespace atomNs = "http://www.w3.org/2005/Atom";

        var entries = atomXml.Root?
            .Elements(atomNs + "entry")
            .Select(entry => new AnimeNews
            {
                Title = entry.Element(atomNs + "title")?.Value ?? "No Title",
                Link = entry.Element(atomNs + "link")?.Attribute("href")?.Value ?? "No Link",
                Published = DateTime.TryParse(entry.Element(atomNs + "published")?.Value, out var pubDate) ? pubDate : DateTime.MinValue,
                Summary = entry.Element(atomNs + "summary")?.Value ?? "No Summary"
            })
            .OrderByDescending(entry => entry.Published)
            .FirstOrDefault();

        return entries;
    }

    private async Task SendNews(AnimeNews news)
    {


        await _botContext.GuildConfigs.LoadAsync();
        foreach (var guild in _botContext.GuildConfigs)
        {
            _botContext.Entry(guild).Reload();
            if (guild.AnimeInfoChannelId == null || guild.AnimeInfoChannelId == 0)
                return;
            else
            {
                ulong channelId = guild.AnimeInfoChannelId ?? 0;
                await _client.GetGuild(guild.Id).GetTextChannel(channelId).SendMessageAsync(embed: BuildNewsMessage(news).Build());
            }
                
        }
    }

    private EmbedBuilder BuildNewsMessage(AnimeNews news)
    {
        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithAuthor(news.Summary, url:news.Link)
            .WithDescription(news.Title)
            .WithFooter(news.Published.ToString())
            .WithImageUrl("https://images.squarespace-cdn.com/content/v1/60b7f1bed70b3159a0b9141d/e8bae26b-a858-4fa0-82b1-244a65af3ac0/anime-news-network-guildmv.jpeg")
            .WithColor(Color.Red);
        return embedBuilder;
    }
}
