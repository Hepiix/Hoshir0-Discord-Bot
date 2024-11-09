using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;
using Newtonsoft.Json;

namespace DiscordNetTemplate.Modules;

public class CookieModule
{
    private readonly DatabaseBotContext _db;
    private readonly List<FortuneCookie>? cookies;

    public CookieModule(DatabaseBotContext db)
    {
        _db = db;
        cookies = JsonConvert.DeserializeObject<List<FortuneCookie>>(File.ReadAllText("data/cookies.json"));
    }

    public string GetCookie(ulong UserId)
    {
        string quote = string.Empty;
        var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
        // User not exists or cookie not saved in db
        if (user == null || user.Cookie == null)
        {
            quote = GetRandomCookie(user, UserId);
        }
        // User have saved cookie
        else
        {
            quote = GetSavedCookie(user);
        }

        return quote;
    }

    private string GetRandomCookie(UserModel user, ulong UserId)
    {
        int random = new Random().Next(cookies.Count);
        string cookie = cookies[random].Quote;

        // User exists
        if (user != null)
        {
            user.Cookie = random;
            _db.SaveChangesAsync();
        }
        // User doesnt exists
        else
        {
            user = new()
            {
                Id = UserId,
                Cookie = random,
                Tarot = null
            };

            _db.Users.Add(user);
            _db.SaveChangesAsync();
        }
        return cookie;
    }

    private string GetSavedCookie(UserModel user)
    {
        return cookies[_db.Users.FirstOrDefault(u => u.Id == user.Id).Cookie ?? 0].Quote;
    }
}
