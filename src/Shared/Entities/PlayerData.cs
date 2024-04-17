using System.Runtime.Serialization;

namespace Shared.Entities;

[DataContract(Name = "GameScore")]
public class PlayerData
{
    [DataMember(Name = "score")] public int score { get; private set; }
    [DataMember(Name = "playerName")] public string playerName { get; private set; }

    public PlayerData(DateTime date, int score, string name)
    {
        this.score = score;
        this.playerName = name;
    }

    public override string ToString()
    {
        return score.ToString();
    }

    public void updateScore(int score)
    {
        this.score = score;
    }
}