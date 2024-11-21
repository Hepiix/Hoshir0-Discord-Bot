using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNetTemplate.Db;
using DiscordNetTemplate.Services;
using Microsoft.EntityFrameworkCore;

namespace DiscordNetTemplate.Modules;

public class TimerTasks
{
    private readonly TimerService _timerService;
    private readonly ILogger _logger;
    private readonly DatabaseBotContext _botContext;

    public TimerTasks(TimerService timerService, ILogger<DiscordBotService> logger, DatabaseBotContext botContext)
    {
        _timerService = timerService;
        _logger = logger;
        _botContext = botContext;
    }

    public void StartTasks()
    {
        _logger.LogInformation("Registering tasks...");
        _timerService.RegisterDailyTask(CookieClear, 11, 00);
        _timerService.RegisterDailyTask(TarotClear, 23, 00);
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
}
