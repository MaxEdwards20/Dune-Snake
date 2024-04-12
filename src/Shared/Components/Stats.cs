namespace Shared.Components;

public class Stats : Component
{
    public uint Score { get; set; }
    public uint Kills { get; set; }

    public Stats(uint score, uint kills)
    {
        Score = score;
        Kills = kills;
    }
}
