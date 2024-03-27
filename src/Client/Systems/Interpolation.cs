
using Microsoft.Xna.Framework;
using Shared.Entities;
using System;

namespace Client.Systems
{
    public class Interpolation : Shared.Systems.System
    {
        public Interpolation() :
            base(
                typeof(Shared.Components.Position),
                typeof(Shared.Components.Movement)
                )
        {
        }

        /// <summary>
        /// Interested in entities that have both Movement and Position components,
        /// but not if they have an Input component.  Furthermore, this
        /// system adds an Goal component in order to properly render the
        /// entity's state during the render stage.
        /// </summary>
        public override bool add(Entity entity)
        {
            bool interested = false;
            if (!entity.contains<Shared.Components.Input>())
            {
                if (base.add(entity))
                {
                    interested = true;
                    var position = entity.get<Shared.Components.Position>();
                    entity.add(new Components.Goal(position.position, position.orientation));
                }
            }

            return interested;
        }

        /// <summary>
        /// Move each entity close to its goal.
        /// </summary>
        public override void update(TimeSpan elapsedTime)
        {
            foreach (var entity in m_entities.Values)
            {
                var position = entity.get<Shared.Components.Position>();
                var goal = entity.get<Components.Goal>();

                if (goal.updateWindow > TimeSpan.Zero && goal.updatedTime < goal.updateWindow)
                {
                    goal.updatedTime += elapsedTime;
                    var updateFraction = (float)elapsedTime.Milliseconds / goal.updateWindow.Milliseconds;

                    // Turn first
                    position.orientation = position.orientation - (goal.startOrientation - goal.goalOrientation) * updateFraction;

                    // Then move
                    position.position = new Vector2(
                        position.position.X - (goal.startPosition.X - goal.goalPosition.X) * updateFraction,
                        position.position.Y - (goal.startPosition.Y - goal.goalPosition.Y) * updateFraction);
                }
            }
        }
    }
}
