using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;
using Newtonsoft.Json;

namespace DiscordNetTemplate.Modules;

public class TarotModule
{
    private readonly DatabaseBotContext _db;
    private readonly List<TarotCard> _cards;

    public TarotModule(DatabaseBotContext db)
    {
        _db = db;
        _cards = JsonConvert.DeserializeObject<List<TarotCard>>(File.ReadAllText("data/tarotcards.json"));
    }

    public bool HasUserUsedTarot (ulong userId, ulong guildId)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
            if (user.TarotGuildIds.Contains(guildId))
                return true;
        else
            return false;

        return false;
    }

    public void SaveGuild(ulong userId, ulong guildId)
    {
        _db.Users.FirstOrDefault(u => u.Id == userId).TarotGuildIds.Add(guildId);
        _db.SaveChangesAsync();
    }

    public TarotCard GetTarotCard(ulong userId)
    {
        TarotCard card = null;
        var user = _db.Users.FirstOrDefault(u =>u.Id == userId);

        if (user == null || user.Tarot == null)
            card = GetRandomCard(user, userId);
        else
            card = GetSavedTarotCard(user);

        return card;
    }

    private TarotCard GetRandomCard(UserModel user, ulong userId)
    {
        int random = new Random().Next(_cards.Count);
        TarotCard card = _cards[random];

        if (user != null)
        {
            user.Tarot = random;
            _db.SaveChangesAsync();
        }
        else
        {
            user = new()
            {
                Id = userId,
                Cookie = null,
                Tarot = random
            };

            _db.Users.Add(user);
            _db.SaveChangesAsync();
        }

        return card;
    }
    
    private TarotCard GetSavedTarotCard(UserModel user)
    {
        return _cards[_db.Users.FirstOrDefault(u => u.Id == user.Id).Tarot ?? 0];
    }

    public string GetTarotCardName (int cardId)
    {
        if (cardId == -1)
            return "Brak";
        return _cards[cardId].Name;
    }
}
