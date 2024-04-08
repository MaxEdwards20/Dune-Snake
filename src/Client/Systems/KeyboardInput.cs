
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using System;
using System.Collections.Generic;
using Shared.Components;
using Client.Components;
using Shared.Systems;

namespace Client.Systems
{
    public class KeyboardInput : Shared.Systems.System
    {
        private HashSet<Keys> m_keysPressed = new HashSet<Keys>();
        private KeyboardState m_statePrevious = Keyboard.GetState();
        private Controls m_controls;

        public KeyboardInput(List<Tuple<Shared.Components.Input.Type, Keys>> mapping, Controls controls) : base(typeof(Shared.Components.Worm))
        {
            m_controls = controls;
        }

        public override void update(TimeSpan elapsedTime)
        {
            if (!m_controls.UseKeyboard)
            {
                return;
            }
            var keyboardState = Keyboard.GetState();
            m_keysPressed.Clear();

            foreach (var key in keyboardState.GetPressedKeys())
            {
                m_keysPressed.Add(key);
            }

            // We have a dictionary of entities, so we need to iterate through them
            foreach (var entity in m_entities)
            {
                if (!entity.Value.contains<Input>())
                {
                    continue;
                }
                var inputs = new List<Input.Type>();
                var worm = WormMovement.getWormFromHead(entity.Value, m_entities);
                if (keyNewlyPressed(m_controls.SnakeLeft.key))
                {
                    inputs.Add(Input.Type.RotateLeft);
                    Shared.Systems.WormMovement.ninetyLeft(worm, elapsedTime);
                }
                if (keyNewlyPressed(m_controls.SnakeRight.key))
                {
                    inputs.Add(Input.Type.RotateRight);
                    Shared.Systems.WormMovement.ninetyRight(worm, elapsedTime);
                }
                if (inputs.Count > 0)
                {
                    // Assuming you have a messaging system to handle input
                    MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(entity.Key, inputs, elapsedTime));
                }
            }
            // Move the current state to the previous state for the next time around
            m_statePrevious = keyboardState;
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
