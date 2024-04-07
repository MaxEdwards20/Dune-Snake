using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Shared.Components;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Client.Components;


namespace Client.Systems;

public class ControlsPersistence: Shared.Systems.System
{
    // Now we want to make all of these control settings persist across game sessions. We will use the same serialization technique we used for the high scores.
    private bool saving = false;
    private bool loading = false;
    private Controls m_loadedState = new Controls();

    public override void update(TimeSpan timeSpan)
    {
        return;
    }

    public void SaveControls(Controls controls)
    {
        if (!saving) {
            saving = true;
            finalizeSaveControlsAsync(controls);
        }
    }

    public void LoadControls(Controls controls) { 
        if (!loading) {
            loading = true;
            var res = finalizeLoadControlsAsync();
            res.Wait(); // we want to load the controls before letting the user start playing
            // All of them have a default value in case they were not saved
            controls.SnakeLeft = m_loadedState.SnakeLeft == null? new Control(Keys.Left) : m_loadedState.SnakeLeft;
            controls.SnakeRight = m_loadedState.SnakeRight == null? new Control(Keys.Right) : m_loadedState.SnakeRight;
            // controls.UseKeyboard = m_loadedState.UseKeyboard == null ? true: m_loadedState.UseKeyboard; // Real. 
            controls.UseKeyboard = true; // FOR TESTING. Allow us to switch keyboard on and off.
        }
    }
    private async Task finalizeSaveControlsAsync(Controls controls)
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile("Controls.json", FileMode.Create))
                        {
                            if (fs != null)
                            {
                                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Controls));
                                mySerializer.WriteObject(fs, controls);
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
    private async Task finalizeLoadControlsAsync()
    {
        await Task.Run(() =>
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (storage.FileExists("Controls.json")) // check if it exists before trying to open it
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile("Controls.json", FileMode.Open))
                        {
                            if (fs != null)
                            {
                                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Controls));
                                m_loadedState = (Controls)mySerializer.ReadObject(fs);
                            }
                        }
                    }
                }
                catch (IsolatedStorageException)
                {
                }
            }

            this.loading = false;
        });
    }
    

}