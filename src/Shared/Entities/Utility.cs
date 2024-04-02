using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities;


// NOTE: We will probably move this over to the wormMovement system where behavior lives
public class Utility
{
    // Everything that hits these endpoints SHOULD be a WormHead (start of a LinkedList of worm parts)
    public static void thrust(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        var head = getHead(entity, entities);
        var snake = getSnakeFromHead(head, entities);
        applyThrust(snake, elapsedTime);
    }

    public static void rotateLeft(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        var head = getHead(entity, entities);
        applyLeftRotation(head, elapsedTime);
    }

    public static void rotateRight(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        var head = getHead(entity, entities);
        applyRightRotation(head, elapsedTime);
    }
    
    private static Entity getHead(Entity entity, Dictionary<uint, Entity> entities)
    {
        var current = entity;
        while (current.contains<ParentId>() && entities.ContainsKey(current.get<ParentId>().id))
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
            if (current.contains<ChildId>() && entities.ContainsKey(current.get<ChildId>().id))
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
            if (current.contains<ParentId>() && entities.ContainsKey(current.get<ParentId>().id))
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
    
    private static void applyThrust(List<Entity> snake, TimeSpan elapsedTime)
    {
        if (snake == null || snake.Count == 0)
        {
            return; // Early exit if snake is empty
        }

        // Calculate the movement vector for the head
        var head = snake[0];
        var headPosition = head.get<Position>();
        var movement = head.get<Movement>();
        var vectorX = Math.Cos(MathHelper.ToRadians(headPosition.orientation));
        var vectorY = Math.Sin(MathHelper.ToRadians(headPosition.orientation));
        var movementVector = new Vector2(
            (float)(vectorX * movement.moveRate * elapsedTime.TotalSeconds),
            (float)(vectorY * movement.moveRate * elapsedTime.TotalSeconds));

        // Store the previous position of the head to calculate the offset for the next segment
        Vector2 previousPosition = headPosition.position;
        float previousOrientation = headPosition.orientation;
        // Update the head position
        headPosition.position += movementVector;

        // Update each body segment's position based on the offset from its predecessor
        for (int i = 1; i < snake.Count; i++)
        {
            var segment = snake[i];
            var segmentPosition = segment.get<Position>();

            // Calculate the offset for the current segment
            Vector2 currentPosition = segmentPosition.position;
            // Apply the offset from the previous segment to this one
            segmentPosition.position = previousPosition + (currentPosition - previousPosition);
            segmentPosition.orientation = previousOrientation;

            // Update previousPosition for the next segment in the list
            previousPosition = currentPosition;
            previousOrientation = segmentPosition.orientation;
        }
    }

    
    // We don't need to update the entire snake with these because it will be updated in the next frame when thrust is applied
    private static void applyLeftRotation(Entity head, TimeSpan elapsedTime)
    {
        var position = head.get<Position>();
        var movement = head.get<Movement>();
        position.orientation -= movement.rotateRate * elapsedTime.Milliseconds;
    }

    private static void applyRightRotation(Entity head, TimeSpan elapsedTime)
    {
        var position = head.get<Position>();
        var movement = head.get<Movement>();
        position.orientation += movement.rotateRate * elapsedTime.Milliseconds;
    }
}