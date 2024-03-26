using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Systems;

namespace Shared.Entities
{
    public class Snake
    {
        public static Entity create(string texture, Vector2 position, float size, float moveRate, float rotateRate, Controls controls = null)
        {
            Entity entity = new Entity();
            if (controls == null)
            {
                controls = new Controls();
            }
            entity.add(controls);
            entity.add(new Appearance(texture));
            entity.add(new Position(position));
            entity.add(new Size(new Vector2(size, size)));
            entity.add(new Movement(moveRate, rotateRate));
            
            List<Input.Type> inputs = new List<Input.Type>();
            inputs.Add(Input.Type.SnakeUp);
            inputs.Add(Input.Type.RotateLeft);
            inputs.Add(Input.Type.RotateRight);
            inputs.Add(Input.Type.SnakeDown);
            inputs.Add(Input.Type.Boost);
            entity.add(new Input(inputs));

            return entity;
        }
    }

    public class Utility
    {
        public static void thrust(Entity entity, TimeSpan elapsedTime)
        {
            var position = entity.get<Position>();
            var movement = entity.get<Movement>();

            var vectorX = Math.Cos(position.orientation);
            var vectorY = Math.Sin(position.orientation);

            position.position = new Vector2(
                (float)(position.position.X + vectorX * movement.moveRate * elapsedTime.Milliseconds),
                (float)(position.position.Y + vectorY * movement.moveRate * elapsedTime.Milliseconds));
        }

        public static void rotateLeft(Entity entity, TimeSpan elapsedTime)
        {
            var position = entity.get<Position>();
            var movement = entity.get<Movement>();

            position.orientation = position.orientation - movement.rotateRate * elapsedTime.Milliseconds;
        }

        public static void rotateRight(Entity entity, TimeSpan elapsedTime)
        {
            var position = entity.get<Position>();
            var movement = entity.get<Movement>();

            position.orientation = position.orientation + movement.rotateRate * elapsedTime.Milliseconds;
        }
        
        public static void boost(Entity entity, TimeSpan elapsedTime)
        {
            var position = entity.get<Position>();
            var movement = entity.get<Movement>();

            var vectorX = Math.Cos(position.orientation);
            var vectorY = Math.Sin(position.orientation);

            position.position = new Vector2(
                (float)(position.position.X + vectorX * movement.moveRate * elapsedTime.Milliseconds * 2),
                (float)(position.position.Y + vectorY * movement.moveRate * elapsedTime.Milliseconds * 2));
        }
    }
}
