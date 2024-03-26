using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

// Added to support serialization
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Client.Objects{

[DataContract(Name = "GameScores")]
public class GameScores { 
    private bool saving = false;
    private bool loading = false;
    private GameScores m_loadedState = null;

    [DataMember(Name = "scores")]
    public List<GameScore> scores { get;  private set; }
    
    public GameScores() {
        // set the defaults in case the file is not there
        scores = new List<GameScore>();
        // load the file in, if it exists. This will also update the scores and high score values
        LoadScores();
        // deleteLocalDataAsync().Wait(); // use this for testing purposes
    }
    public void addScore(int score) {
        var today = System.DateTime.Now;
        scores.Add(new GameScore(today, score));
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
    }

    public void removeScore(int score) {
        scores.RemoveAll(x => x.score == score);
    }
    
    public void removeScore(GameScore score) {
        scores.Remove(score);
    }


    public GameScores sortScores(GameScores gameScores, int numToDisplay) {
        
        // Order the list of scores by the GameScore.score property
        var orderedScores = gameScores.scores.OrderByDescending(x => x.score);
        // Take the top numToDisplay scores
        var topScores = orderedScores.Take(numToDisplay);
        // Create a new GameScores object with the top scores
        GameScores topGameScores = new GameScores();
        topGameScores.scores = topScores.ToList();
        return topGameScores;
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

[DataContract(Name = "GameScore")]
public class GameScore {
    [DataMember(Name = "date")]
    public DateTime date { get; private set; }
    [DataMember(Name = "score")]
    public int score { get; private set; }
    public GameScore(DateTime date, int score) {
        this.date = date;
        this.score = score;
    }
}
}
