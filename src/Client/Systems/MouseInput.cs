using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using System;
using System.Collections.Generic;
using Client.Components;
using Shared.Components;

namespace Client.Systems
{
    public class MouseInput : Shared.Systems.System
    {
        private MouseState previousMouseState = Mouse.GetState();
        private Controls m_controls;

        public MouseInput(Controls controls) : base(typeof(Shared.Components.Worm))
        {
            m_controls = controls; 
        }

        public override void update(TimeSpan elapsedTime)
        {
            if (m_controls.UseKeyboard)
            {
                return;
            }

            var mouseState = Mouse.GetState();
            var currentPosition = new Vector2(mouseState.X, mouseState.Y);

            foreach (var entityPair in m_entities)
            {
                var entity = entityPair.Value;
                if (!entity.contains<Input>())
                {
                    continue;
                }
                // Try to get the Position component safely
                try
                {
                    var positionComponent = entity.get<Position>();
                    var wormHeadPosition = positionComponent.position;
                    var direction = currentPosition - wormHeadPosition;
                    var inputs = new List<Input.Type>();

                    // Check mouse movement relative to the worm head's position to decide on turn direction
                    if (direction.X < 0 && currentPosition != previousMouseState.Position.ToVector2()) // Mouse moved left
                    {
                        HandleTurnLeft(entity, elapsedTime);
                        inputs.Add(Input.Type.RotateLeft);
                    }
                    else if (direction.X > 0 && currentPosition != previousMouseState.Position.ToVector2()) // Mouse moved right
                    {
                        HandleTurnRight(entity, elapsedTime);
                        inputs.Add(Input.Type.RotateLeft);
                    }
                    
                    // Always add thrust
                    inputs.Add(Input.Type.SnakeUp);
                    Utility.thrust(entity, elapsedTime, m_entities);
                    
                    if (inputs.Count > 0)
                    {
                        // Assuming you have a messaging system to handle input
                        MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(entityPair.Key, inputs, elapsedTime));
                    }
                }
                catch (KeyNotFoundException)
                {
                    // Component not found, skip this entity
                }
            }

            previousMouseState = mouseState; 
        }

        private void HandleTurnLeft(Entity entity, TimeSpan elapsedTime)
        {
            Utility.rotateLeft(entity, elapsedTime, m_entities);
        }

        private void HandleTurnRight(Entity entity, TimeSpan elapsedTime)
        {
            Utility.rotateRight(entity, elapsedTime, m_entities);
        }
    }
}
