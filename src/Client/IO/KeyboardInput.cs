using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Client.Menu;


namespace Client.IO
{
    /// <summary>
    /// Derived input device for the PC Keyboard
    /// </summary>
    // Added to support serialization


    [DataContract(Name = "KeyboardInput")]
    public class KeyboardInput : IInputDevice
    {
        /// <summary>
        /// Registers a callback-based command
        /// </summary>
        /// 

        // Now we want to make all of these control settings persist across game sessions. We will use the same serialization technique we used for the high scores.
        private bool saving = false;
        private bool loading = false;
        private KeyboardInput m_loadedState = null;
        private KeyboardState m_statePrevious = Keyboard.GetState();

        [DataMember(Name = "SnakeUp")]
        public Control SnakeUp = new Control(Keys.Up);
        [DataMember(Name = "SnakeLeft")]
        public Control SnakeLeft = new Control(Keys.Left);
        [DataMember(Name = "SnakeRight")]
        public Control SnakeRight = new Control(Keys.Right);

        [DataMember(Name = "SnakeDown")]
        public Control SnakeDown = new Control(Keys.Down);
        public Control Up = new Control(Keys.Up);
        public Control Down = new Control(Keys.Down);
        public Control Enter = new Control(Keys.Enter);
        public Control Escape = new Control(Keys.Escape);
        public Control Select = new Control(Keys.Enter);
        private Dictionary<Control, CommandEntry> m_commandEntries = new Dictionary<Control, CommandEntry>();

        public KeyboardInput()
        {
            LoadControls();
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

        public void registerCommand(Control control, bool keyPressOnly, IInputDevice.CommandDelegate callback)
        {
            //
            // If already registered, remove it!
            if (m_commandEntries.ContainsKey(control))
            {
                m_commandEntries.Remove(control);
            }
            m_commandEntries.Add(control, new CommandEntry(control.key, keyPressOnly, callback));
        }

        public void ClearAllCommands()
        {
            m_commandEntries.Clear();
        }

        // get pressed keys
        public Keys[] GetPressedKeys()
        {
            return Keyboard.GetState().GetPressedKeys();
        }


        /// <summary>
        /// Used to keep track of the details associated with a command
        /// </summary>
        private struct CommandEntry
        {
            public CommandEntry(Keys key, bool keyPressOnly, IInputDevice.CommandDelegate callback)
            {
                this.key = key;
                this.keyPressOnly = keyPressOnly;
                this.callback = callback;
            }

            public Keys key;
            public bool keyPressOnly;
            public IInputDevice.CommandDelegate callback;
        }

        /// <summary>
        /// Goes through all the registered commands and invokes the callbacks if they
        /// are active.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            foreach (CommandEntry entry in this.m_commandEntries.Values)
            {
                if (entry.keyPressOnly && keyPressed(entry.key))
                {
                    entry.callback(gameTime, 1.0f);
                }
                else if (!entry.keyPressOnly && state.IsKeyDown(entry.key))
                {
                    entry.callback(gameTime, 1.0f);
                }
            }
            //
            // Move the current state to the previous state for the next time around
            m_statePrevious = state;
        }


        /// <summary>
        /// Checks to see if a key was newly pressed
        /// </summary>
        private bool keyPressed(Keys key)
        {
            return (Keyboard.GetState().IsKeyDown(key) && !m_statePrevious.IsKeyDown(key));
        }

        internal void updateControlKey(ControlSettingsView.ControlStateEnum v, Keys key)
        {
            if (v == ControlSettingsView.ControlStateEnum.SnakeLeft)
            {
                SnakeLeft.switchKey(key);
            }
            else if (v == ControlSettingsView.ControlStateEnum.SnakeRight)
            {
                SnakeRight.switchKey(key);
            }
            else if (v == ControlSettingsView.ControlStateEnum.SnakeUp)
            {
                SnakeUp.switchKey(key);
            } else if (v == ControlSettingsView.ControlStateEnum.SnakeDown)
            {
                SnakeDown.switchKey(key);
            }
            SaveControls();
        }
    
    
        private async Task finalizeSaveAsync(KeyboardInput keyboard)
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
                                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(KeyboardInput));
                                mySerializer.WriteObject(fs, keyboard);
                                
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
                                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(KeyboardInput));
                                m_loadedState = (KeyboardInput)mySerializer.ReadObject(fs);
                                this.SnakeUp = m_loadedState.SnakeUp;
                                this.SnakeLeft = m_loadedState.SnakeLeft;
                                this.SnakeRight = m_loadedState.SnakeRight;
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
}
