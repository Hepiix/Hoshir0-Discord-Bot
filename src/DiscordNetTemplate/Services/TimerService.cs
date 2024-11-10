using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DiscordNetTemplate.Models;
using DiscordNetTemplate.Modules;
using Microsoft.Extensions.Hosting;

namespace DiscordNetTemplate.Services;

public class TimerService
{
    private readonly ConcurrentBag<TimerModel> _tasks;
    private readonly Timer _timer;
    private readonly ILogger _logger;

    public TimerService(ILogger<DiscordBotService> ilogger)
    {
        _tasks = new ConcurrentBag<TimerModel>();
        _timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        _logger = ilogger;
    }

    public void RegisterTask(Func<Task> action, TimeSpan interval)
    {
        _tasks.Add(new TimerModel(action, interval));
    }

    public void RegisterDailyTask(Func<Task> action, int hour, int minute)
    {
        var initialInterval = GetTimeUntil(hour, minute);
        _tasks.Add(new TimerModel(action, TimeSpan.FromDays(1)) { NextRunTime = DateTime.UtcNow + initialInterval });
    }

    private async void TimerCallback(object state)
    {
        var now = DateTime.UtcNow;

        foreach (var task in _tasks)
        {
            if (now >= task.NextRunTime)
            {
                await task.Action();
                task.NextRunTime = now + task.Interval;
                _logger.LogInformation($"Daily task: {now + task.Interval}");
            }
        }
    }

    private TimeSpan GetTimeUntil(int hour, int minute)
    {
        var now = DateTime.UtcNow;
        var nextRunTime = now.Date.AddHours(hour).AddMinutes(minute);

        if (nextRunTime <= now)
            nextRunTime = nextRunTime.AddDays(1);

        _logger.LogInformation((nextRunTime - now).ToString());
        return nextRunTime - now;
    }
}
