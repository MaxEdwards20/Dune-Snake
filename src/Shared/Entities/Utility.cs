using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities;


// NOTE: We will probably move this over to the wormMovement system where behavior lives
public class Utility
{
    public static void thrust(Entity entity, TimeSpan elapsedTime)
    {
        var position = entity.get<Position>();
        var movement = entity.get<Movement>();

        var vectorX = Math.Cos(position.orientation);
        var vectorY = Math.Sin(position.orientation);

        position.position = new Vector2(
            (float)(position.position.X - vectorX * movement.moveRate * elapsedTime.Milliseconds),
            (float)(position.position.Y - vectorY * movement.moveRate * elapsedTime.Milliseconds));
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
}