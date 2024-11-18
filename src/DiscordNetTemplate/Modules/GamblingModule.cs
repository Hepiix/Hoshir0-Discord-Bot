using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNetTemplate.Db;

namespace DiscordNetTemplate.Modules;

public class GamblingModule
{
    private readonly DatabaseBotContext _botContext;

    public GamblingModule(DatabaseBotContext botContext)
    {
        _botContext = botContext;
    }

    public void GiveMoney()
    {

    }
}
