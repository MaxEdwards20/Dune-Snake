using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

//
// Added to support serialization
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace CS5410.Objects{

[DataContract(Name = "GameScores")]
public class GameScores { 
    // Safeguard against multiple save/load happening at the same time
    private bool saving = false;
    private bool loading = false;
    private GameScores m_loadedState = null;

    [DataMember(Name = "scores")]
    public Dictionary<string, int> scores { get;  private set; }
    
    [DataMember(Name = "highScore")]
    public int highScore { get; private set; }
    public GameScores() {
        // set the defaults in case the file is not there
        scores = new Dictionary<string, int>();
        highScore = 0;
        // load the file in, if it exists. This will also update the scores and high score values
        LoadScores();
        // deleteLocalDataAsync().Wait();
    }
    public void addScore(int score) {
        var today = System.DateTime.Now;
        if (scores.ContainsKey(today.ToString())) {
            scores[today.ToString()] = score;
        }
        else {
            scores.Add(today.ToString(), score);
        }
        if (score > highScore) {
            highScore = score;
        }
        SaveScores();
    }

    public void SaveScores() {
        if (!saving) {
            saving = true;
            finalizeSaveAsync(this);
        }
    }

    public void LoadScores() { 
        if (!loading) {
            loading = true;
            finalizeLoadAsync().Wait(); // we want to load the scores completely to ensure we have the most recent data
        }
    }
    public void clearScores() {
        scores.Clear();
        highScore = 0;
    }

    public void removeScore(int score) {
        foreach (var key in scores.Keys) {
            if (scores[key] == score) {
                scores.Remove(key);
                break;
            }
        }
    }


    public List<(int, string)> SortScores(Dictionary<string, int> scores, int numDisplayScores) {
        // Sort the scores by value
        List<(int, string)> sortedScores = new List<(int, string)>();
        foreach (var score in scores) {
            var formattedDate = score.Key.Split(' ')[0];
            sortedScores.Add((score.Value, formattedDate));
        }
        sortedScores.Sort((x, y) => y.Item1.CompareTo(x.Item1));
        sortedScores = sortedScores.Take(numDisplayScores).ToList();
        return sortedScores;
    }

    // Used to delete the local data for testing purposes (or if you're embarrased about your score)
    private async Task deleteLocalDataAsync()
    {
        await Task.Run(() =>
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    storage.DeleteFile("HighScores.json");
                }
                catch (IsolatedStorageException)
                {
                }
            }
        });
    }

    private async Task finalizeSaveAsync(GameScores gameScores)
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Create))
                        {
                            if (fs != null)
                            {
                                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(GameScores));
                                mySerializer.WriteObject(fs, gameScores);
                                
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        
                    }
                }

                this.saving = false;
            });
        }

    private async Task finalizeLoadAsync()
    {
        await Task.Run(() =>
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (storage.FileExists("HighScores.json")) // check it exists before trying to open it
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Open))
                        {
                            if (fs != null)
                            {
                                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(GameScores));
                                m_loadedState = (GameScores)mySerializer.ReadObject(fs);
                                scores = m_loadedState.scores;
                                highScore = m_loadedState.highScore;
                            }
                        }
                    }
                }
                catch (IsolatedStorageException)
                {
                    // Ideally show something to the user, but this is demo code :)
                }
            }

            this.loading = false;
        });
    }
}
}
