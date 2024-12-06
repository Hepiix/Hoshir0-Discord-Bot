using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;

namespace DiscordNetTemplate.Modules
{
    public class ConfigModule
    {
        private readonly DatabaseBotContext _db;

        public ConfigModule(DatabaseBotContext db)
        {
            _db = db;
        }

        public void SaveGamblingChannel(ulong guildId, ulong channelId)
        {
            var guild = _db.GuildConfigs.FirstOrDefault(u => u.Id == guildId);
            if (guild == null)
            {
                GuildConfigModel model = new()
                {
                    Id = guildId,
                    GamblingChannel = channelId
                };

                _db.GuildConfigs.Add(model);
            }
            else
            {
                guild.GamblingChannel = channelId;
            }
            _db.SaveChangesAsync();
        }

        public bool CheckIfGamblingAllowed(ulong guildId, ulong channelId)
        {
            var guild = _db.GuildConfigs.FirstOrDefault(u => u.Id == guildId);
            if (guild == null || guild.GamblingChannel == 0 || guild.GamblingChannel == null || channelId != guild.GamblingChannel)
            {
                return false;
            }
            else
                return true;
        }
    }
    
}
