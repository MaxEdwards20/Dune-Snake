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

    // Core 4 directions
    private static float RIGHT_Radians = 0;
    private static float UP_Radians = -MathHelper.PiOver2;
    private static float DOWN_Radians = MathHelper.PiOver2;
    private static float LEFT_Radians = MathHelper.Pi;
    // Diagonal 4 directions
    private static float UP_RIGHT_Radians = -MathHelper.PiOver4;
    private static float UP_LEFT_Radians = -3 * MathHelper.PiOver4;
    private static float DOWN_RIGHT_Radians = MathHelper.PiOver4;
    private static float DOWN_LEFT_Radians = 3 * MathHelper.PiOver4;
    
    
    public static void up(List<Entity> snake)
    {
        if (!isWithinAngleThreshold(snake[0].get<Position>().orientation, DOWN_Radians))
        {
            changeDirection(snake, UP_Radians);
        }
    }
    
    public static void down(List<Entity> snake)
    {
        if (!isWithinAngleThreshold(snake[0].get<Position>().orientation, UP_Radians))
        {
            changeDirection(snake, DOWN_Radians);
        }
    }
    
    public static void left(List<Entity> snake, TimeSpan elapsedTime)
    {
        if (!isWithinAngleThreshold(snake[0].get<Position>().orientation, RIGHT_Radians))
        {
            changeDirection(snake, LEFT_Radians);
        }
    }
    
    public static void right(List<Entity> snake, TimeSpan elapsedTime)
    {
        if (!isWithinAngleThreshold(snake[0].get<Position>().orientation, LEFT_Radians))
        {
            changeDirection(snake, RIGHT_Radians);
        }
    }
    public static void upLeft(List<Entity> snake)
    {
        if (!isWithinAngleThreshold( snake[0].get<Position>().orientation, DOWN_RIGHT_Radians))
        {
            changeDirection(snake, UP_LEFT_Radians);
        }
    }
    
    public static void upRight(List<Entity> snake)
    {
        if (!isWithinAngleThreshold(snake[0].get<Position>().orientation, DOWN_LEFT_Radians))
        {
            changeDirection(snake, UP_RIGHT_Radians);
        }
    }
    
    public static void downLeft(List<Entity> snake)
    {
        if (!isWithinAngleThreshold(snake[0].get<Position>().orientation, UP_RIGHT_Radians))
        {
            changeDirection(snake, DOWN_LEFT_Radians);
        }
    }
    
    public static void downRight(List<Entity> snake)
    {
        if (!isWithinAngleThreshold(snake[0].get<Position>().orientation, UP_LEFT_Radians))
        {
            changeDirection(snake, DOWN_RIGHT_Radians);
        }
    }

    private void applyThrust(Entity wormHead, TimeSpan elapsedTime)
    {
        // Setup variables
        var snake = getWormFromHead(wormHead, m_entities);
        var head = snake[0];
        var movement = head.get<Movement>();
        var headPosition = head.get<Position>();
        var orientation = headPosition.orientation;
        float LOCATION_THRESHOLD = movement.moveRate * 20;
        const float MIN_SEGMENT_SPACING = 40f;
        const float IDEAL_SEGMENT_SPACING = 50f;


        // Move the head
        var direction = new Vector2((float)Math.Cos(orientation), (float)Math.Sin(orientation));
        direction.Normalize();
        headPosition.position += direction * movement.moveRate * (float)elapsedTime.TotalMilliseconds;;
        
        // Move the rest of the worm
        for (int i = 1; i < snake.Count; i++)
        {
            var entity = snake[i];
            var queueComponent = entity.get<AnchorQueue>();
            var currentPosition = entity.get<Position>();
            var parent = snake[i - 1];
            var parentPosition = parent.get<Position>();
            var entityFrameMovement = movement.moveRate * (float)elapsedTime.TotalMilliseconds;

            // Default moving towards parent position
            var target = new Position(parentPosition.position, parentPosition.orientation);

            while (queueComponent.m_anchorPositions.Count > 0 && entityFrameMovement > 0)
            {
                target = queueComponent.m_anchorPositions.Peek();
                var distanceToTarget = Vector2.Distance(currentPosition.position, target.position);
                
                // Move towards the target
                if (distanceToTarget > entityFrameMovement)
                {
                    var directionToTarget = target.position - currentPosition.position;
                    directionToTarget.Normalize();
                    currentPosition.position += directionToTarget * entityFrameMovement;
                    entityFrameMovement = 0;
                }
                else
                {
                    // Move to the target
                    currentPosition.position = target.position;
                    entityFrameMovement -= distanceToTarget;
                    queueComponent.m_anchorPositions.Dequeue();
                }
            }
            if (entityFrameMovement > 0)
            {
                target = new Position(parentPosition.position, parentPosition.orientation);
                var distanceToTarget = Vector2.Distance(currentPosition.position, target.position);
                // We want to move towards the parent but not so close we are on top of it
                if (distanceToTarget > IDEAL_SEGMENT_SPACING)
                {
                    var directionToParent = parentPosition.position - currentPosition.position;
                    directionToParent.Normalize();
                    currentPosition.position += directionToParent * entityFrameMovement;
                }
            }
        }
    }
    
    private static bool isWithinAngleThreshold(float radians, float targetRadians, float threshold = MathHelper.PiOver4/2)
    {
        // We need to normalize the angles to ensure they are within the same range and both positive
        radians = (radians + MathHelper.TwoPi) % MathHelper.TwoPi;
        targetRadians = (targetRadians + MathHelper.TwoPi) % MathHelper.TwoPi;
        var diff = Math.Abs(radians - targetRadians);
        // If the difference is greater than PI, we need to use the complementary angle
        if (diff > MathHelper.Pi)
        {
            diff = MathHelper.TwoPi - diff;
        }
        return diff < threshold;
    }
    
    private static void changeDirection(List<Entity> worm, float radians)
    {
            if (worm == null || worm.Count == 0 || isWithinAngleThreshold(worm[0].get<Position>().orientation, radians)) 
                return;

            // Assuming the first entity in the list is the head
            var head = worm[0];
            var headPosition = head.get<Position>();
            headPosition.orientation = radians;

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