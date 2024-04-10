using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;

namespace Server.Systems;

public class CollisionDetection : Shared.Systems.System
{
    private System.Action<uint> m_removeEntity;
    private System.Action<Entity> m_addEntity;
    private System.Action<Entity> m_updateEntity;
    
    private List<Entity> m_newEntities = new List<Entity>();
    private List<Entity> m_updatedEntities = new List<Entity>();
    private List<Entity> m_removedEntities = new List<Entity>();
    
    public static int POWER_TO_GROW = 50;
    
    public CollisionDetection() :
        base(
            typeof(Shared.Components.Collision), typeof(Shared.Components.Position), typeof(Shared.Components.Size)
        )
    {
    }
    
    public override void update(TimeSpan elapsedTime)
    {
        m_newEntities.Clear();
        m_updatedEntities.Clear();
        m_removedEntities.Clear();
        // Get the heads of the worms
        List<Entity> heads = new List<Entity>();
        foreach (var entity in m_entities.Values)
        {
            if (entity.contains<Head>())
            {
                heads.Add(entity);
            }
        }
        
        // Check each head against everything else
        foreach (var head in heads)
        {
            var worm = WormMovement.getWormFromHead(head, m_entities);
            // Make this worm into a hashset
            HashSet<uint> wormSet = new HashSet<uint>();
            foreach (var entity in worm)
            {
                wormSet.Add(entity.id);
            }
            foreach (var entity in m_entities.Values)
            {
                // Ignore elements of the worm against everyone else
                if (wormSet.Contains(entity.id))
                {
                    continue;
                }
                
                if (entity.contains<SpicePower>() && !entity.contains<Worm>() )
                {
                    if (CircleCircleIntersect(
                        head.get<Position>().position,
                        head.get<Size>().size.X,
                        entity.get<Position>().position,
                        entity.get<Size>().size.X
                    ))
                    {
                        handleWormAteSpice(head, entity, elapsedTime);
                    }
                }
                else if (entity.contains<Worm>())
                {
                    if (CircleCircleIntersect(
                        head.get<Position>().position,
                        head.get<Size>().size.X,
                        entity.get<Position>().position,
                        entity.get<Size>().size.X
                    ))
                    {
                        handleWormAteWorm(worm, entity);
                    }
                }
                else if (entity.contains<Shared.Components.Wall>())
                {
                    // Entity is a wall
                    var wallPos = entity.get<Position>().position;
                    var wallSize = entity.get<Size>().size;
                    Vector2 topLeft = wallPos;
                    Vector2 topRight = new Vector2(wallPos.X + wallSize.X, wallPos.Y);
                    Vector2 bottomLeft = new Vector2(wallPos.X, wallPos.Y + wallSize.Y);
                    Vector2 bottomRight = wallPos + wallSize;

// Check each side of the wall for intersection with the snake head
                    if (CircleLineIntersect(head.get<Position>().position, head.get<Size>().size.X / 2, topLeft, topRight) ||
                        CircleLineIntersect(head.get<Position>().position, head.get<Size>().size.X / 2, topLeft, bottomLeft) ||
                        CircleLineIntersect(head.get<Position>().position, head.get<Size>().size.X / 2, bottomLeft, bottomRight) ||
                        CircleLineIntersect(head.get<Position>().position, head.get<Size>().size.X / 2, topRight, bottomRight))
                    {
                        handleWormHitWall(worm);
                    }
                }
            }
        }
        processNewEntities();
        processUpdatedEntities();
        processRemovedEntities();
    }
    
    // Reference: https://stackoverflow.com/questions/37224912/circle-line-segment-collision
    private bool CircleLineIntersect(Vector2 circleCenter, float circleRadius, Vector2 lineStart, Vector2 lineEnd)
    {
        // Find the closest point on the line segment to the center of the circle
        Vector2 lineVec = lineEnd - lineStart;
        Vector2 circleToLineStartVec = circleCenter - lineStart;
        float t = Vector2.Dot(circleToLineStartVec, lineVec) / Vector2.Dot(lineVec, lineVec);
        t = MathHelper.Clamp(t, 0, 1); // Clamp t to the range [0, 1] to stay within the line segment
        Vector2 closestPoint = lineStart + t * lineVec;

        // Check if the closest point is within the circle
        float distanceSquared = Vector2.DistanceSquared(circleCenter, closestPoint);
        return distanceSquared <= (circleRadius * circleRadius);
    }
    
    private static bool CircleCircleIntersect(Vector2 position1, float radius1, Vector2 position2, float radius2)
    {
        return Vector2.Distance(position1, position2) < radius1 + radius2;
    }
    
    private void handleWormAteSpice(Entity head, Entity spice, TimeSpan elapsedTime)
    {
        // Remove the spice
        m_removeEntity(spice.id);
        MessageQueueServer.instance.broadcastMessage(new RemoveEntity(spice.id));
        // Add power to the worm head
        var headPower = head.get<SpicePower>();
        var spicePower = spice.get<SpicePower>();
        headPower.addPower(spicePower.power);
        // Now we check if the head has grown enough to add a new segment
        if (headPower.power >= POWER_TO_GROW)
        {
            headPower.resetPower();
            // Add a new segment directly behind the worm head
            var worm = WormMovement.getWormFromHead(head, m_entities);
            var headPos = head.get<Position>().position;
            var segmentPos = new Vector2(headPos.X , headPos.Y);
            // update the head pos to be more forward
            var headSize = head.get<Size>().size.X;
            var headRotation = head.get<Position>().orientation;
            headPos.X += (float) Math.Cos(headRotation) * headSize;
            headPos.Y += (float) Math.Sin(headRotation) * headSize;
            var newSegment = WormSegment.create(segmentPos, head.id);
            var headChild = head.get<ChildId>();
            newSegment.add(new ChildId(headChild.id)); // now the new segment is between the head and the previous child segment
            // now we update this previous child segment to point to the new segment
            var oldChild = m_entities[headChild.id];
            oldChild.remove<ParentId>();
            oldChild.add(new ParentId(newSegment.id));
            
            // now update the heads child
            head.remove<ChildId>();
            head.add(new ChildId(newSegment.id));
            worm.Insert(1, newSegment);
            m_newEntities.Add(newSegment);
            m_updatedEntities.AddRange(worm);
            m_updatedEntities.Add(oldChild);
        }
        else
        {
            MessageQueueServer.instance.broadcastMessage(new UpdateEntity(head, elapsedTime));
        }

    }
    
    private void handleWormAteWorm(List<Entity> worm, Entity otherHead)
    {
        // Check if we hit head on head
        if (otherHead.contains<Head>())
        {
            // We need to compare the sizes of the two worms to see who dies
            List<Entity> otherWorm = WormMovement.getWormFromHead(otherHead, m_entities);
            if (worm.Count > otherWorm.Count)
            {
                m_removedEntities.AddRange(otherWorm);
            }
            else
            {
                m_removedEntities.AddRange(worm);
            }
        }
        else // We hit the side of the worm
        {
            // If the worm hit the body, then the worm dies
            m_removedEntities.AddRange(worm);
        }
    }
    
    private void handleWormHitWall(List<Entity> worm)
    {
        m_removedEntities.AddRange(worm);
    }
    
    public void registerRemoveEntity(Action<uint> removeEntity)
    {
        m_removeEntity = removeEntity;
    }
    public void registerUpdateEntity(Action<Entity> updateEntity)
    {
        m_updateEntity = updateEntity;
    }
    
    public void registerAddEntity(Action<Entity> addEntity)
    {
        m_addEntity = addEntity;
    }
    
    private void processNewEntities()
    {
        foreach (var entity in m_newEntities)
        {
            m_addEntity(entity);
            MessageQueueServer.instance.broadcastMessage(new NewEntity(entity));
        }
    }
    
    private void processUpdatedEntities()
    {
        foreach (var entity in m_updatedEntities)
        {
            // m_updateEntity(entity);
            MessageQueueServer.instance.broadcastMessage(new UpdateEntity(entity, TimeSpan.Zero));
        }
    }
    
    private void processRemovedEntities()
    {
        foreach (var entity in m_removedEntities)
        {
            m_removeEntity(entity.id);
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(entity.id));
        }
    }
    
    
}


