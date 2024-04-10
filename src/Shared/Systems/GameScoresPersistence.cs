using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using Shared.Components;
using Shared.Entities;

namespace Shared.Systems;

public class GameScoresPersistence
{
    private bool saving = false;
    private bool loading = false;
    
    private GameScores m_loadedState = new GameScores();


    public void SaveScores(GameScores gameScores)
    {
        if (!saving)
        {
            saving = true;
            finalizeSaveGameScoresAsync(gameScores);
        }
    }

    public GameScores LoadScores() { 
        if (!loading) {
            loading = true;
            finalizeLoadGameScoresAsync().Wait(); // we want to load the scores completely to ensure we have the most recent data
        }
        return m_loadedState;
    }
    
    // Used to delete the local data for testing purposes (or if you're embarrased about your score)
    private async Task deleteLocalGameScoresAsync()
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

    private async Task finalizeSaveGameScoresAsync(GameScores gameScores)
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

    private async Task finalizeLoadGameScoresAsync()
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