using System.Runtime.Serialization;

namespace Shared.Entities;

[DataContract(Name = "GameScore")]
public class PlayerData
{
    public int clientId { get; set; }
    [DataMember(Name = "score")] public int score { get; set; }
    [DataMember(Name = "playerName")] public string playerName { get; private set; }

    public PlayerData(int score, string name)
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