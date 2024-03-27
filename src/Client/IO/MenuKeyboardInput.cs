using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Client.Menu;
using Shared.Components;


namespace Client.IO
{
    /// <summary>
    /// Derived input device for the PC Keyboard
    /// </summary>
    // Added to support serialization


    public class MenuKeyboardInput : IInputDevice
    {
        private KeyboardState m_statePrevious = Keyboard.GetState();
        /// <summary>
        /// Registers a callback-based command
        /// </summary>
        /// 



        public Control Up = new Control(Keys.Up);
        public Control Down = new Control(Keys.Down);
        public Control Enter = new Control(Keys.Enter);
        public Control Escape = new Control(Keys.Escape);
        public Control Select = new Control(Keys.Enter);
        private Dictionary<Control, CommandEntry> m_commandEntries = new Dictionary<Control, CommandEntry>();

        public MenuKeyboardInput()
        {
        }



        public void registerCommand(Control control, bool keyPressOnly, IInputDevice.CommandDelegate callback)
        {
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
    }
}
