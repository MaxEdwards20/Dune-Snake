
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
                checkAndPerformAction(m_controls.SnakeUp, Input.Type.SnakeUp, entity.Value, elapsedTime, inputs);
                checkAndPerformAction(m_controls.SnakeLeft, Input.Type.RotateLeft, entity.Value, elapsedTime, inputs);
                checkAndPerformAction(m_controls.SnakeRight, Input.Type.RotateRight, entity.Value, elapsedTime, inputs);
                
                if (inputs.Count > 0)
                {
                    // Assuming you have a messaging system to handle input
                    MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(entity.Key, inputs, elapsedTime));
                }
            }
            // Move the current state to the previous state for the next time around
            m_statePrevious = keyboardState;
        }
        
        private void checkAndPerformAction(Control control, Input.Type inputType, Entity entity, TimeSpan elapsedTime, List<Input.Type> inputs)
        {
            if (m_keysPressed.Contains(control.key))
            {
                inputs.Add(inputType);
                // Perform action based on inputType
                switch (inputType)
                {
                    case Input.Type.SnakeUp:
                        Utility.thrust(entity, elapsedTime);
                        break;
                    case Input.Type.RotateLeft:
                        Utility.rotateLeft(entity, elapsedTime);
                        break;
                    case Input.Type.RotateRight:
                        Utility.rotateRight(entity, elapsedTime);
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
        private bool keyPressed(Keys key)
        {
            return (Keyboard.GetState().IsKeyDown(key) && !m_statePrevious.IsKeyDown(key));
        }

    }
}
