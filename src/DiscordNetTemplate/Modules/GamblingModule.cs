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
    private readonly Random _random = new Random();

    public GamblingModule(DatabaseBotContext botContext)
    {
        _db = botContext;
    }

    private static readonly List<(string Symbol, int Weight)> SymbolWeights = new List<(string Symbol, int Weight)>
    {
        ("🍒", 30),
        (":lemon:", 25),
        (":watermelon:", 15),
        (":pineapple:", 12),
        (":bell:", 10),
        (":gem:", 5),
        (":seven:", 2),
        ("jackpot", 1)
    };

    private readonly Dictionary<string, float> symbolValues = new()
    {
    { "🍒", 2f },
    { ":lemon:", 4f },
    { ":watermelon:", 6f },
    { ":pineapple:", 8f },
    { ":bell:", 10f },
    { ":gem:", 20f },
    { ":seven:", 50f },
    { "jackpot", 500f }
    };

    private readonly Dictionary<string, float> symbolPartialMultipliers = new Dictionary<string, float>
{
    { "🍒", 0.4f },
    { ":lemon:", 0.6f },
    { ":watermelon:", 1.0f },
    { ":pineapple:", 1.3f },
    { ":bell:", 1.8f },
    { ":gem:", 2.5f },
    { ":seven:", 4.0f },
    { "jackpot", 5.0f }
};

    private string GetRandomSymbol()
    {
        int totalWeight = SymbolWeights.Sum(s => s.Weight);
        int roll = _random.Next(totalWeight);

        int cumulative = 0;
        foreach (var (symbol, weight) in SymbolWeights)
        {
            cumulative += weight;
            if (roll < cumulative)
                return symbol;
        }

        return SymbolWeights.Last().Symbol;
    }

    public string[] GenerateFinalRoll()
    {
        string[] result = new string[3];

        for (int i = 0; i < 3; i++)
        {
            result[i] = GetRandomSymbol();
        }

        return result;
    }

    public string CheckWin(string[] finalRoll, int bet, ulong userId)
    {
        var symbolCount = finalRoll.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());
        int winAmount = 0;
        string message = "";

        if (symbolCount.Count == 1)
        {
            string symbol = finalRoll[0];
            float multiplier = symbolValues.ContainsKey(symbol) ? symbolValues[symbol] : 1f;

            winAmount = (int)Math.Floor(bet * multiplier);

            message = $"🎉 BIG WIN! Trafiłeś 3x {symbol}! Wygrywasz {winAmount} żetonów!";
        }
        else if (symbolCount.Any(kv => kv.Value == 2))
        {
            var pair = symbolCount.First(kv => kv.Value == 2).Key;
            float multiplier = symbolPartialMultipliers.ContainsKey(pair) ? symbolPartialMultipliers[pair] : 0f;
            winAmount = (int)Math.Floor(bet * multiplier);

            if (winAmount == 0)
                return "Masz dwa takie same symbole, ale są zbyt słabe, by coś wygrać.";

            message = $":confetti_ball: Masz 2x {pair}! Wygrywasz {winAmount} żetonów!";
        }
        else
        {
            return "❌ Niestety, tym razem nic nie wygrałeś. Spróbuj ponownie!";
        }

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.Money += winAmount;
            _db.SaveChangesAsync();
        }

        return $"{message} Twój nowy stan konta: {user?.Money ?? 0}";
    }

    public string AddMoney(ulong userId, int money)
    {
        if (_random.NextDouble() > 0.7)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Money += money;
                _db.SaveChangesAsync();
                return $"🎁 Wygrałeś dodatkowe {money} żetonów!";
            }
        }
        return "";
    }
}
