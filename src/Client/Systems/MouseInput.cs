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
        private MouseState m_previousState = Mouse.GetState();

        public MouseInput() : base(typeof(Shared.Components.Input))
        {
        }

        public override void update(TimeSpan elapsedTime)
        {
            var currentState = Mouse.GetState();

            if (currentState.Position != m_previousState.Position)
            {
                foreach (var entity in m_entities)
                {
                    // Here, we need to determine the direction for the snake to move
                    var inputs = new List<Input.Type> { Input.Type.FollowMouse };

                    MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(entity.Key, inputs, elapsedTime));
                }
            }

            m_previousState = currentState;
        }
    }
}
