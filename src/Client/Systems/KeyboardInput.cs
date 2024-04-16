
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
        private Controls m_controls = new Controls(); // Default value that is overwritten in the constructor

        public KeyboardInput(List<Tuple<Shared.Components.Input.Type, Keys>> mapping, Controls controls) : base(
            typeof(Shared.Components.Worm))
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
                if (!entity.Value.contains<Input>())
                {
                    continue;
                }

                var inputs = new List<Input.Type>();
                var worm = WormMovement.getWormFromHead(entity.Value, m_entities);
                // Start with the combinations
                if (keysNewlyPressed(m_controls.SnakeUp.key, m_controls.SnakeLeft.key))
                {
                    inputs.Add(Input.Type.PointUpLeft);
                    Shared.Systems.WormMovement.upLeft(worm);
                }
                else if (keysNewlyPressed( m_controls.SnakeUp.key, m_controls.SnakeRight.key ))
                {
                    inputs.Add(Input.Type.PointUpRight);
                    Shared.Systems.WormMovement.upRight(worm);
                }
                else if (keysNewlyPressed( m_controls.SnakeDown.key,  m_controls.SnakeLeft.key ))
                {
                    inputs.Add(Input.Type.PointDownLeft);
                    Shared.Systems.WormMovement.downLeft(worm);
                }
                else if (keysNewlyPressed( m_controls.SnakeDown.key, m_controls.SnakeRight.key ))
                {
                    inputs.Add(Input.Type.PointDownRight);
                    Shared.Systems.WormMovement.downRight(worm);
                }
                else if (keyNewlyPressed(m_controls.SnakeLeft.key))
                {
                    inputs.Add(Input.Type.PointLeft);
                    Shared.Systems.WormMovement.left(worm, elapsedTime);
                }
                else if (keyNewlyPressed(m_controls.SnakeRight.key))
                {
                    inputs.Add(Input.Type.PointRight);
                    Shared.Systems.WormMovement.right(worm, elapsedTime);
                }
                else if (keyNewlyPressed(m_controls.SnakeUp.key))
                {
                    inputs.Add(Input.Type.PointUp);
                    Shared.Systems.WormMovement.up(worm);
                }
                else if (keyNewlyPressed(m_controls.SnakeDown.key))
                {
                    inputs.Add(Input.Type.PointDown);
                    Shared.Systems.WormMovement.down(worm);
                }


                if (inputs.Count > 0)
                {
                    // Assuming you have a messaging system to handle input
                    MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(entity.Key, inputs,
                        elapsedTime));
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

        private bool keysNewlyPressed(Keys key1, Keys key2)
        {
            return keyPressed(key1) && keyPressed(key2) && (!m_statePrevious.IsKeyDown(key1) ||
                   !m_statePrevious.IsKeyDown(key2));
        }
    }
}


