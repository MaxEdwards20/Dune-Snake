using System;
using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;

namespace Shared.Systems;

public class WormMovement : Shared.Systems.System 
{
    public WormMovement() : base(typeof(Shared.Components.Worm))
    {
    }
    
    public override void update(TimeSpan elapsedTime) 
    {
        // Get all of the heads of the worms
        var heads = new List<Entity>();
        foreach (var entityPair in m_entities)
        {
            var entity = entityPair.Value;
            if (entity.contains<Head>())
            {
                heads.Add(entity);;
            }
        }
        
        // Apply thrust to all of the worms
        foreach (var head in heads)
        {
            thrust(head, elapsedTime, m_entities);
        }
    }
    
        // The entity that hits these endpoints should be the head of the worm, with the rest of the worm in the entities
    private List<Entity> thrust(Entity head, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        var snake = getSnakeFromHead(head, entities);
        applyThrust(snake, elapsedTime);
        return snake;
    }

    public static void ninetyLeft(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        var head = getHead(entity, entities);
        applyLeftRotation(head, -90);
    }

    public static void ninetyRight(Entity entity, TimeSpan elapsedTime, Dictionary<uint, Entity> entities)
    {
        var head = getHead(entity, entities);
        applyRightRotation(head, 90);
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

    
    // We don't need to update the entire worm with these because it will be updated in the next frame when thrust is applied
    private static void applyLeftRotation(Entity head, int degrees)
    {
        var position = head.get<Position>();
        var movement = head.get<Movement>();
        // Rotate left 90 degrees
        position.orientation += degrees;
    }

    private static void applyRightRotation(Entity head, int degrees)
    {
        var position = head.get<Position>();
        var movement = head.get<Movement>();
        // Rotate right 90 degrees
        position.orientation += degrees;
    }
    
}