using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using System;
using System.Collections.Generic;
using Shared.Components;

namespace Client.Systems
{
    public class MouseInput : Shared.Systems.System
    {
        private MouseState previousMouseState = Mouse.GetState();
        private Controls m_controls;

        public MouseInput(Controls controls) : base(typeof(Shared.Components.Input))
        {
            m_controls = controls; 
        }

        public override void update(TimeSpan elapsedTime)
        {
            var mouseState = Mouse.GetState();
            var currentPosition = new Vector2(mouseState.X, mouseState.Y);

            foreach (var entityPair in m_entities)
            {
                var entity = entityPair.Value;
                // Try to get the Position component safely
                try
                {
                    var positionComponent = entity.get<Position>();

                    var wormHeadPosition = positionComponent.position;
                    var direction = currentPosition - wormHeadPosition;

                    // Check mouse movement relative to the worm head's position to decide on turn direction
                    if (direction.X < 0 && currentPosition != previousMouseState.Position.ToVector2()) // Mouse moved left
                    {
                        HandleTurnLeft(entity, elapsedTime);
                    }
                    else if (direction.X > 0 && currentPosition != previousMouseState.Position.ToVector2()) // Mouse moved right
                    {
                        HandleTurnRight(entity, elapsedTime);
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
            // Logic for left turn
        }

        private void HandleTurnRight(Entity entity, TimeSpan elapsedTime)
        {
            // Logic for right turn
        }
    }
}
