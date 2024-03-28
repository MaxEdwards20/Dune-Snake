using System.Runtime.Serialization;

namespace Shared.Components;

[DataContract(Name = "GameScore")]
public class PlayerData: Component
{
    [DataMember(Name = "date")] public DateTime date { get; private set; }
    [DataMember(Name = "score")] public int score { get; private set; }
    [DataMember(Name = "playerName")] public string playerName { get; private set; }

    public PlayerData(DateTime date, int score, string name)
    {
        this.date = date;
        this.score = score;
        this.playerName = name;
    }

    public override string ToString()
    {
        return date.ToString() + " " + score.ToString();
    }

    public void updateScore(int score)
    {
        this.score = score;
    }
}