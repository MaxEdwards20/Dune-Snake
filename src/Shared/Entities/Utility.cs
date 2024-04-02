using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities;


// NOTE: We will probably move this over to the wormMovement system where behavior lives
public class Utility
{
    // Everything that hits these endpoints SHOULD be a WormHead (start of a LinkedList of worm parts)
    public static void thrust(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        entity = getHead(entity, entities);
        var position = entity.get<Position>();
        var movement = entity.get<Movement>();

        var vectorX = Math.Cos(position.orientation);
        var vectorY = Math.Sin(position.orientation);

        position.position = new Vector2(
            (float)(position.position.X - vectorX * movement.moveRate * elapsedTime.Milliseconds),
            (float)(position.position.Y - vectorY * movement.moveRate * elapsedTime.Milliseconds));
    }

    public static void rotateLeft(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        entity = getHead(entity, entities);
        var position = entity.get<Position>();
        var movement = entity.get<Movement>();

        position.orientation = position.orientation - movement.rotateRate * elapsedTime.Milliseconds;
    }

    public static void rotateRight(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        entity = getHead(entity, entities);
        var position = entity.get<Position>();
        var movement = entity.get<Movement>();

        position.orientation = position.orientation + movement.rotateRate * elapsedTime.Milliseconds;
    }
    
    private static Entity getHead(Entity entity, Dictionary<uint, Entity> entities)
    {
        var current = entity;
        while (current.contains<ParentId>())
        {
            current = entities[current.get<ParentId>().id];
        }
        return current;
    }
    
    private static List<Entity> getSnakeFromHead(Entity head, Dictionary<uint, Entity> entities)
    {
        var snakeEntities = new List<Entity>();
        var current = head;
        while (current != null)
        {
            snakeEntities.Add(current);
            if (current.contains<ChildId>())
            {
                current = entities[current.get<ChildId>().id];
            }
            else
            {
                current = null;
            }
        }
        return snakeEntities;
    }
    
    private static List<Entity> getSnakeFromTail(Entity tail, Dictionary<uint, Entity> entities)
    {
        var snakeEntities = new List<Entity>();
        var current = tail;
        while (current != null)
        {
            snakeEntities.Add(current);
            if (current.contains<ParentId>())
            {
                current = entities[current.get<ParentId>().id];
            }
            else
            {
                current = null;
            }
        }
        return snakeEntities;
    }
    
}