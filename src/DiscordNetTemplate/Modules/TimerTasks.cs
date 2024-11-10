﻿using System;
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
        _timerService.RegisterDailyTask(CookieClear, 12, 00);
    }

    public async Task CookieClear()
    {
        foreach(var user in _botContext.Users)
        {
            user.Cookie = null;
            user.CookieGuildIds = new List<ulong>();

            _botContext.Entry(user).Property(u => u.Cookie).IsModified = true;
            _botContext.Entry(user).Property(u => u.CookieGuildIds).IsModified = true;
        }
        _botContext.SaveChangesAsync();
        _logger.LogInformation("Db saved");
    }
}
