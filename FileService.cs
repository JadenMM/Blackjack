using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blackjack;

public class FileService
{

    private static readonly string FilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/ArbutusInteractive/Blackjack/";

    public static void SaveStats(Stats stats)
    {
        string json = JsonSerializer.Serialize(stats);

        if (!Directory.Exists(FilePath))
            Directory.CreateDirectory(FilePath);

        File.WriteAllText(FilePath + "stats.json", json);
    }

    public static Stats LoadStats()
    {
        if (!File.Exists(FilePath + "stats.json"))
        {
            var newStat = new Stats();
            SaveStats(newStat);
            return newStat;
        }

        string raw = File.ReadAllText(FilePath + "stats.json");

        var stats = JsonSerializer.Deserialize<Stats>(raw);

        return stats;
    }

}

[Serializable]
public class Stats
{
    public float Coins { get; set; } = 500;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int Blackjacks { get; set; }

    public void ShowStats()
    {
        Console.WriteLine("Player Stats");
        Console.WriteLine($"Coins: {Coins}");
        Console.WriteLine($"Blackjacks: {Blackjacks}");
        Console.WriteLine($"Wins: {Wins}");
        Console.WriteLine($"Ties: {Ties}");
        Console.WriteLine($"Losses: {Losses}");
    }
}
