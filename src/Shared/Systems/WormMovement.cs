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
            applyThrust(head, elapsedTime);
            applyThrust(head, elapsedTime);
            applyThrust(head, elapsedTime);
        }
    }
    

    public static void ninetyLeft(Entity head, TimeSpan elapsedTime)
    {
        applyLeftRotation(head, -MathHelper.PiOver2);
    }

    public static void ninetyRight(Entity head, TimeSpan elapsedTime)
    {
        applyRightRotation(head, MathHelper.PiOver2);
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
    
    private List<Entity> getSnakeFromHead(Entity head, Dictionary<uint, Entity> entities)
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
    
    private List<Entity> getSnakeFromTail(Entity tail)
    {
        var snakeEntities = new List<Entity>();
        var current = tail;
        while (current != null)
        {
            snakeEntities.Add(current);
            if (current.contains<ParentId>() && m_entities.ContainsKey(current.get<ParentId>().id))
            {
                current = m_entities[current.get<ParentId>().id];
            }
            else
            {
                current = null;
            }
        }
        return snakeEntities;
    }


    // TODO: Add a periodic 2/second or maybe 10/second updates of the position to the server
    // TODO: Transition to using the queue of positions each entity should move towards
    private void applyThrust(Entity wormHead, TimeSpan elapsedTime)
    {
        // Get the movement component of the head to determine the direction and speed
        var movementComponent = wormHead.get<Movement>();
        var positionComponent = wormHead.get<Position>();

        // Calculate the new position of the head based on its direction and speed
        Vector2 direction = new Vector2((float) Math.Cos(positionComponent.orientation),
            (float) Math.Sin(positionComponent.orientation));
        direction.Normalize(); // Ensure the direction vector is normalized
        Vector2 newPosition = positionComponent.position - direction * movementComponent.moveRate * elapsedTime.Milliseconds;

        // Update the head's position
        positionComponent.position = newPosition;

        // Now, update each following segment to move towards the position of its preceding segment
        Entity currentSegment = wormHead; // Start with the head
        Vector2 previousPosition = newPosition; // Position to move towards, start with the new head position

        while (true)
        {
            // Assuming each segment has a ParentId component that points to its preceding segment
            if (!currentSegment.contains<ChildId>())
            {
                break; // No more segments in the chain
            }
            
            // Get the next segment in the chain
            uint childId = currentSegment.get<ChildId>().id;
            var nextSegment = m_entities[childId];

            // Temporarily store the current segment's position to use for the next segment
            Vector2 tempPosition = nextSegment.get<Position>().position;

            // Move the current segment towards the previous position
            nextSegment.get<Position>().position = previousPosition;

            // Prepare for the next iteration
            previousPosition = tempPosition;
            currentSegment = nextSegment;
        }
    }



    // We don't need to update the entire worm with these because it will be updated in the next frame when thrust is applied
    private static void applyLeftRotation(Entity head, float radians)
    {
        var position = head.get<Position>();
        var movement = head.get<Movement>();
        position.orientation += radians;
    }

    private static void applyRightRotation(Entity head, float radians)
    {
        var position = head.get<Position>();
        var movement = head.get<Movement>();
        position.orientation += radians;
    }
    
}