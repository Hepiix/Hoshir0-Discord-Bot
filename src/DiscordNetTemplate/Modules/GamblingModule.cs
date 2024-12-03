using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNetTemplate.Db;
using DiscordNetTemplate.Models;

namespace DiscordNetTemplate.Modules;

public class GamblingModule
{
    private readonly DatabaseBotContext _db;

    public GamblingModule(DatabaseBotContext botContext)
    {
        _db = botContext;
    }

    private readonly string[] symbols = { "🍒", "🍒", "🍒", "🍒", "🍋", "🍋", "🍎", "🍎", ":strawberry:", ":strawberry:", "💎", "⭐" };

    private readonly Dictionary<string, int> symbolValues = new Dictionary<string, int>
    {
        { "🍒", 2 },
        { ":strawberry:", 3 },
        { "🍋", 3 },
        { "🍎", 4 },
        { "💎", 10 },
        { "⭐", 7 } 
    };

    public string[] GenerateFinalRoll()
    {
        var random = new Random();
        string[] result = new string[3];

        for (int i = 0; i < 3; i++)
        {
            result[i] = symbols[random.Next(symbols.Length)];
        }

        return result;
    }

    public string CheckWin(string[] finalRoll, int bet, ulong userId)
    {
        var symbolCount = finalRoll.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());

        if (symbolCount.Count == 1)
        {
            string winningSymbol = finalRoll[0];
            int multiplier = symbolValues[winningSymbol] * 2;
            bet *= multiplier;
            var userMoney = _db.Users.FirstOrDefault(u => u.Id == userId).Money += bet;
            _db.SaveChangesAsync();
            return $"Gratulacje! Wszystkie symbole to {winningSymbol}! Wygrałeś {bet} żetonów! Twoja łączna ilość żetonów: {userMoney}";
        }

        if (symbolCount.Count == 2)
        {
            var pairSymbol = symbolCount.FirstOrDefault(s => s.Value == 2).Key;
            int multiplier = (int)Math.Round(symbolValues[pairSymbol] / 2.5f);
            bet *= multiplier;
            var userMoney = _db.Users.FirstOrDefault(u => u.Id == userId).Money += bet;
            _db.SaveChangesAsync();
            return $"Masz dwa takie same symbole! {pairSymbol} wygrywasz {bet} żetonów! Twoja łączna ilość żetonów: {userMoney}";
        }
        return "Niestety, tym razem nic nie wygrałeś. Spróbuj ponownie!";
    }

    public string AddMoney(ulong userId, int money)
    {
        Random random = new Random();
        double randomValue = random.NextDouble();

        if (randomValue > 0.7)
        {
            _db.Users.FirstOrDefault(u => u.Id == userId).Money += money;
            _db.SaveChangesAsync();
            return $"Wygrałeś {money} żetonów!";
        }
        else
        {
            return "";
        }

        
    }
}
