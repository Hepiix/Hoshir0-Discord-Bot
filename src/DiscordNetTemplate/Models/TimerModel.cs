using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNetTemplate.Models;

public class TimerModel
{
    public Func<Task> Action { get; set; }
    public TimeSpan Interval { get; set; }
    public DateTime NextRunTime { get; set; }

    public TimerModel(Func<Task> action, TimeSpan interval)
    {
        Action = action;
        Interval = interval;
        NextRunTime = DateTime.UtcNow + interval;
    }
}
