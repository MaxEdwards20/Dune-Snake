
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using System;
using System.Collections.Generic;
using Shared.Components;

namespace Client.Systems
{
    public class KeyboardInput : Shared.Systems.System
    {
        private HashSet<Keys> m_keysPressed = new HashSet<Keys>();
        private KeyboardState m_statePrevious = Keyboard.GetState();
        private Controls m_controls;

        public KeyboardInput(List<Tuple<Shared.Components.Input.Type, Keys>> mapping, Controls controls) : base(typeof(Shared.Components.Input))
        {
            m_controls = controls;
        }

        public override void update(TimeSpan elapsedTime)
        {
            var keyboardState = Keyboard.GetState();
            m_keysPressed.Clear();

            foreach (var key in keyboardState.GetPressedKeys())
            {
                m_keysPressed.Add(key);
            }

            // We have a dictionary of entities, so we need to iterate through them
            foreach (var entity in m_entities)
            {
                var inputs = new List<Input.Type>();
                inputs.Add(Input.Type.SnakeUp);
                if (keyPressed(m_controls.SnakeLeft.key))
                {
                    inputs.Add(Input.Type.RotateLeft);
                }
                if (keyPressed(m_controls.SnakeRight.key))
                {
                    inputs.Add(Input.Type.RotateRight);
                }
                
                // Now we handle the input locally before sending the message to the server
                performInputAction(entity.Value, elapsedTime, inputs);

                if (inputs.Count > 0)
                {
                    // Assuming you have a messaging system to handle input
                    MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(entity.Key, inputs, elapsedTime));
                }
            }
            // Move the current state to the previous state for the next time around
            m_statePrevious = keyboardState;
        }
        
        private void performInputAction(Entity entity, TimeSpan elapsedTime, List<Input.Type> inputs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var inputType = inputs[i];
                // Perform action based on inputType
                // NOTE: Could do an optimization here where we have all of the combinations of possible inputs
                switch (inputType)
                {
                    case Input.Type.SnakeUp:
                        Utility.thrust(entity, elapsedTime, m_entities);
                        break;
                    case Input.Type.RotateLeft:
                        Utility.rotateLeft(entity, elapsedTime, m_entities);
                        break;
                    case Input.Type.RotateRight:
                        Utility.rotateRight(entity, elapsedTime, m_entities);
                        break;
                }
            }
        }
        
        public override bool add(Entity entity)
        {
            if (!base.add(entity))
            {
                return false;
            }
            return true;
        }

        public override void remove(uint id)
        {
            base.remove(id);
        }
        
        /// <summary>
        /// Checks to see if a key was newly pressed
        /// </summary>
        private bool keyNewlyPressed(Keys key)
        {
            return (Keyboard.GetState().IsKeyDown(key) && !m_statePrevious.IsKeyDown(key));
        }

        private bool keyPressed(Keys key)
        {
            return m_keysPressed.Contains(key);
        }
    }
}
