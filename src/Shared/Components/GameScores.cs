using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

// Added to support serialization
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Shared.Components{

[DataContract(Name = "GameScores")]
public class GameScores: Component {
    [DataMember(Name = "scores")]
    public List<PlayerData> scores { get;  private set; }
    public GameScores() {
        // set the defaults in case the file is not there
        scores = new List<PlayerData>();
    }
    
    public void addScore(int score, string name) {
        var today = System.DateTime.Now;
        scores.Add(new PlayerData(today, score, name));
    }
    
    public void clearScores() {
        scores.Clear();
    }

    public void removeScore(int score) {
        scores.RemoveAll(x => x.score == score);
    }
    
    public void removeScore(PlayerData data) {
        scores.Remove(data);
    }
    
    public void sortScores() {
        // Order the list of scores by the GameScore.score property
        scores = scores.OrderByDescending(x => x.score).ToList();
    }
}


}
