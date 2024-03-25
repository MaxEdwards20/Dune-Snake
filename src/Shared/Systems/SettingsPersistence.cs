using Shared.Messages;

using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using System;
using System.Collections.Generic;
using Shared.Components;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Shared.Systems;

public class SettingsPersistence: System
{
    // Now we want to make all of these control settings persist across game sessions. We will use the same serialization technique we used for the high scores.
    private bool saving = false;
    private bool loading = false;
    private Controls m_loadedState;

    public override void update(TimeSpan timeSpan)
    {
        return;
    }

    public void SaveControls() { 
        if (!saving) {
            saving = true;
            finalizeSaveAsync(this);
        }
    }

    public void LoadControls() { 
        if (!loading) {
            loading = true;
            var res = finalizeLoadAsync();
            res.Wait(); // we want to load the controls before letting the user start playing
        }
    }
    
            // TODO: Add persistence code here
        private async Task finalizeSaveAsync(Controls controls)
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        // TODO: Check if this is the correct filename
                        using (IsolatedStorageFileStream fs = storage.OpenFile("Controls.json", FileMode.Create))
                        {
                            if (fs != null)
                            {
                                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(KeyboardInput));
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
    private async Task finalizeLoadAsync()
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