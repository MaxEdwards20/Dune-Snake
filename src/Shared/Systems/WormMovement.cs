using System;
using System.Diagnostics;
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
        // Update our invincibily while here
        foreach (var entity in m_entities.Values)
        {
            if (entity.contains<Invincible>())
            {
                var invincible = entity.get<Invincible>();
                invincible.update((int)elapsedTime.TotalMilliseconds);
                if (invincible.duration <= 0)
                {
                    entity.remove<Invincible>();
                }
            }
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

    private static float UP_Radians = -MathHelper.PiOver2;
    private static float DOWN_Radians = MathHelper.PiOver2;
    
    public static void upLeft(List<Entity> snake)
    {
        changeDirection(snake, UP_Radians - MathHelper.PiOver4);
    }
    
    public static void upRight(List<Entity> snake)
    {
        changeDirection(snake, UP_Radians + MathHelper.PiOver4);
    }
    
    public static void downRight(List<Entity> snake)
    {
        changeDirection(snake, DOWN_Radians - MathHelper.PiOver4);
    }

    public static void downLeft(List<Entity> snake)
    {
        changeDirection(snake, DOWN_Radians + MathHelper.PiOver4);
    }
    
    public static void left(List<Entity> snake, TimeSpan elapsedTime)
    {
        changeDirection(snake, MathHelper.Pi);
    }
    
    public static void up(List<Entity> snake)
    {
        changeDirection(snake, UP_Radians);
    }
    
    public static void down(List<Entity> snake)
    {
        changeDirection(snake, DOWN_Radians);
    }

    public static void right(List<Entity> snake, TimeSpan elapsedTime)
    {
        changeDirection(snake, MathHelper.TwoPi);
    }

    private void applyThrust(Entity wormHead, TimeSpan elapsedTime)
    {
        // Setup variables
        var snake = getWormFromHead(wormHead, m_entities);
        var head = snake[0];
        var movement = head.get<Movement>();
        var headPosition = head.get<Position>();
        var frameTotalMovement = movement.moveRate * (float)elapsedTime.TotalMilliseconds;
        var orientation = headPosition.orientation;
        float LOCATION_THRESHOLD = movement.moveRate * 20;
        const float MIN_SEGMENT_SPACING = 40f;
        const float IDEAL_SEGMENT_SPACING = 50f;
        
        
        // Move the head
        var direction = new Vector2((float)Math.Cos(orientation), (float)Math.Sin(orientation));
        direction.Normalize();
        headPosition.position += direction * frameTotalMovement;
        
        // Move the rest of the worm
        for (int i = 1; i < snake.Count; i++)
        {
            var entity = snake[i];
            var queueComponent = entity.get<AnchorQueue>();
            var currentPosition = entity.get<Position>();
            var parent = snake[i - 1];
            var parentPosition = parent.get<Position>();
            
            // Default moving towards parent position
            var target = new Position(parentPosition.position, parentPosition.orientation);

            if (queueComponent.m_anchorPositions.Count != 0)
            {
                // queueComponent.m_anchorPositions.Enqueue(new Position(parentPosition.position, parentPosition.orientation));
                target = queueComponent.m_anchorPositions.Peek();
            }
            // Move towards that target
            var distanceToTarget = Vector2.Distance(currentPosition.position, target.position);
            var distanceToParent = Vector2.Distance(currentPosition.position, parentPosition.position);
            if (distanceToTarget >= MIN_SEGMENT_SPACING || distanceToParent >= IDEAL_SEGMENT_SPACING)
            {
                var directionToTarget = target.position - currentPosition.position;
                directionToTarget.Normalize();
                currentPosition.position += directionToTarget * frameTotalMovement;

                // Update the orientation to match the direction we're moving
                currentPosition.orientation = (float)Math.Atan2(directionToTarget.Y, directionToTarget.X);

                // Check if we have hit the target
                if (Vector2.Distance(currentPosition.position, target.position) <= LOCATION_THRESHOLD && queueComponent.m_anchorPositions.Count > 0)
                {
                    // Remove the target from the queue
                    queueComponent.m_anchorPositions.Dequeue();
                }
            }
        }
    }
    
    private static void changeDirection(List<Entity> worm, float radians)
    {
            if (worm == null || worm.Count == 0) 
                return;

            // Assuming the first entity in the list is the head
            var head = worm[0];
            var headPosition = head.get<Position>();
            
            // Adjust the head's orientation by the specified radians and that is it not 180
            var oppositeDirction = (radians + MathHelper.Pi) % (2 * Math.PI);
            var headIsOppositeDirection = Math.Abs(headPosition.orientation - oppositeDirction) < 0.1;
            if (headPosition.orientation == radians || headIsOppositeDirection) return;
            headPosition.orientation = radians;

            // Normalize the orientation to ensure it stays within a valid range (e.g., 0 to 2*PI)
            // This step is important if your system expects orientations within a specific range
            headPosition.orientation = (float)(headPosition.orientation % (2 * Math.PI));
            if (headPosition.orientation < 0) headPosition.orientation += (float)(2 * Math.PI);

            // For each segment, add the current head position to its queue for following
            // Skip the head itself, start with the first segment following the head
            for (int i = 1; i < worm.Count; i++)
            {
                var segment = worm[i];
                var queueComponent = segment.get<AnchorQueue>();
                // Here, we're adding the head's current position as the new target for each segment
                // This mimics the behavior where, upon rotation, each segment should aim to move
                // towards the position where the head was at the time of rotation
                queueComponent.m_anchorPositions.Enqueue(new Position(headPosition.position, headPosition.orientation));
                
            }
    }
    
    public static Entity getHead(Entity entity, Dictionary<uint, Entity> entities)
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
        // make sure we are at the head
        while (head.contains<ParentId>() && entities.ContainsKey(head.get<ParentId>().id))
        {
            head = entities[head.get<ParentId>().id];
        }

        var worm = new List<Entity>();
        var current = head;
        while (current != null)
        {
            worm.Add(current);
            if (current.contains<ChildId>() && entities.ContainsKey(current.get<ChildId>().id))
            {
                current = entities[current.get<ChildId>().id];
            }
            else
            {
                current = null;
            }
        }
        Debug.Assert(worm[0].contains<Head>(), "The first entity in the list should be the head");
        return worm;
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