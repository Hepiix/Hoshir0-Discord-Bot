using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNetTemplate.Models;

public class GuildConfigModel
{
    public ulong Id { get; set; }
    public ulong AnimeInfoChannelId { get; set; }
    public bool AntiSpam { get; set; }
    public bool Gambling { get; set; }
}
