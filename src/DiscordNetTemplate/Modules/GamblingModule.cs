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
        (":lemon:", 27),
        (":watermelon:", 20),
        (":pineapple:", 17),
        (":bell:", 12),
        (":gem:", 9),
        (":seven:", 5),
        (":black_joker:", 2)
    };

    private readonly Dictionary<string, float> symbolValues = new()
    {
    { "🍒", 3f },
    { ":lemon:", 6f },
    { ":watermelon:", 9f },
    { ":pineapple:", 12f },
    { ":bell:", 15f },
    { ":gem:", 30f },
    { ":seven:", 75f },
    { ":black_joker:", 750f }
    };

    private readonly Dictionary<string, float> symbolPartialMultipliers = new Dictionary<string, float>
{
    { "🍒", 0.6f },
    { ":lemon:", 0.8f },
    { ":watermelon:", 1.2f },
    { ":pineapple:", 1.5f },
    { ":bell:", 2f },
    { ":gem:", 2.7f },
    { ":seven:", 4.2f },
    { ":black_joker:", 5.2f }
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
