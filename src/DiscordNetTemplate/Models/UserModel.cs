using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNetTemplate.Models;

public class UserModel
{
    public ulong Id { get; set; }
    public int? Tarot { get; set; }
    public List<ulong>? TarotGuildIds { get; set; } = new List<ulong>();
    public int? Cookie { get; set; }
    public List<ulong>? CookieGuildIds { get; set; } = new List<ulong>();
    public int Money { get; set; }
}
