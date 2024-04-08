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
        var heads = getHeads();
        // Apply thrust to all of the worms
        foreach (var head in heads)
        {
            applyThrust(head, elapsedTime);
        }
    }

    private List<Entity> getHeads()
    {
        var heads = new List<Entity>();
        foreach (var entityPair in m_entities)
        {
            var entity = entityPair.Value;
            if (entity.contains<Head>())
            {
                heads.Add(entity);
            }
        }
        return heads;
    }

    public static void ninetyLeft(List<Entity> snake, TimeSpan elapsedTime)
    {
        applyLeftRotation(snake, MathHelper.PiOver2);
    }

    public static void ninetyRight(List<Entity> snake, TimeSpan elapsedTime)
    {
        applyRightRotation(snake, MathHelper.PiOver2);
    }
    

    // TODO: Transition to using the queue of positions each entity should move towards
    private void applyThrust(Entity wormHead, TimeSpan elapsedTime)
    {
        var snake = getWormFromHead(wormHead, m_entities);
        var head = snake[0];
        var movement = head.get<Movement>();
        var headPosition = head.get<Position>();
        var thrust = movement.moveRate * (float)elapsedTime.TotalMilliseconds;
        var orientation = headPosition.orientation;
        var direction = new Vector2((float)Math.Cos(orientation), (float)Math.Sin(orientation));
        var thrustVector = direction * thrust;
        headPosition.position -= thrustVector;
        
        for (int i = 1; i < snake.Count; i++)
        {
            var entity = snake[i];
            var queueComponent = entity.get<AnchorQueue>();
            var positionComponent = entity.get<Position>();
            if (queueComponent != null)
            {
                if (queueComponent.m_anchorPositions.Count == 0)
                {
                    // Add the current position to the queue
                    queueComponent.m_anchorPositions.Enqueue(headPosition);
                }
                
                // Check where we want to move towards
                var target = queueComponent.m_anchorPositions.Peek();
                
                // Move towards that position
                orientation = positionComponent.orientation;
                var targetOrientation = target.orientation;
                var directionToTarget = new Vector2((float)Math.Cos(targetOrientation), (float)Math.Sin(targetOrientation));
                var moveVector = directionToTarget * thrust;
                positionComponent.position += moveVector;
                

                // Check if we have hit the target
                if (Vector2.Distance(positionComponent.position, target.position) < 0.2f && queueComponent.m_anchorPositions.Count > 0)
                {
                    // Remove the target from the queue
                    queueComponent.m_anchorPositions.Dequeue();
                }
            }
        }
    }

    private static void applyLeftRotation(List<Entity> snake, float radians)
    {
        applyRotation(snake, -radians);
    }
    
    private static void applyRightRotation(List<Entity> snake, float radians)
    {
        applyRotation(snake, radians);
    }
    
    private static void applyRotation(List<Entity> snake, float radians)
    {
        if (snake == null || snake.Count == 0) return;

            // Assuming the first entity in the list is the head
            var head = snake[0];
            var headPosition = head.get<Position>();

            // Adjust the head's orientation by the specified radians
            headPosition.orientation += radians;

            // Normalize the orientation to ensure it stays within a valid range (e.g., 0 to 2*PI)
            // This step is important if your system expects orientations within a specific range
            headPosition.orientation = (float)(headPosition.orientation % (2 * Math.PI));
            if (headPosition.orientation < 0) headPosition.orientation += (float)(2 * Math.PI);

            // For each segment, add the current head position to its queue for following
            // Skip the head itself, start with the first segment following the head
            for (int i = 1; i < snake.Count; i++)
            {
                var segment = snake[i];
                var queueComponent = segment.get<AnchorQueue>();

                if (queueComponent != null)
                {
                    // Here, we're adding the head's current position as the new target for each segment
                    // This mimics the behavior where, upon rotation, each segment should aim to move
                    // towards the position where the head was at the time of rotation
                    queueComponent.m_anchorPositions.Enqueue(new Position(headPosition.position, headPosition.orientation));
                }
            }
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
    
    public static List<Entity> getWormFromHead(Entity head, Dictionary<uint, Entity> entities)
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
    
    public static List<Entity> getSnakeFromTail(Entity tail, Dictionary<uint, Entity> entities)
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
    
}