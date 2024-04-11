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
    
    private HashSet<Entity> m_entsToCreate = new ();
    private HashSet<Entity> m_entsToUpdate = new ();
    private HashSet<Entity> m_entsToRemove = new ();
    
    public static int POWER_TO_GROW = 50;
    
    public CollisionDetection() :
        base(
            typeof(Shared.Components.Collidable), typeof(Shared.Components.Position), typeof(Shared.Components.Size)
        )
    {
    }
    
    public override void update(TimeSpan elapsedTime)
    {
        m_entsToCreate.Clear();
        m_entsToUpdate.Clear();
        m_entsToRemove.Clear();
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
                    wormToSpice(elapsedTime, head, entity);
                }
                else if (entity.contains<Worm>())
                {
                    wormToWorm(head, entity, worm);
                }
                else if (entity.contains<Shared.Components.Wall>())
                {
                    wormToWall(entity, head, worm);
                }
            }
        }
        processNewEntities();
        processUpdatedEntities(elapsedTime);
        processRemovedEntities();
    }

    private void wormToWall(Entity entity, Entity head, List<Entity> worm)
    {
        if (worm[0].contains<Invincible>()) return; // NOTE: Could potentially add some logic here to stop the worm from moving or to turn it around if it hits a wall
        // Entity is a wall
        var wallPos = entity.get<Position>().position;
        var wallSize = entity.get<Size>().size;
        Vector2 topLeft = wallPos;
        Vector2 topRight = new Vector2(wallPos.X + wallSize.X, wallPos.Y);
        Vector2 bottomLeft = new Vector2(wallPos.X, wallPos.Y + wallSize.Y);
        Vector2 bottomRight = wallPos + wallSize;
        var headPos = head.get<Position>().position;
        var headSize = head.get<Size>().size.X /4;

// Check each side of the wall for intersection with the snake head
        if (CircleLineIntersect(headPos, headSize, topLeft, topRight) ||
            CircleLineIntersect(headPos, headSize, topLeft, bottomLeft) ||
            CircleLineIntersect(headPos, headSize, bottomLeft, bottomRight) ||
            CircleLineIntersect(headPos, headSize, topRight, bottomRight))
        {
            handleWormHitWall(worm, entity);
        }
    }

    private void wormToWorm(Entity head, Entity entity, List<Entity> worm)
    {
        if (head.contains<Invincible>() || entity.contains<Invincible>()) return; // This says that I can't eat or be eaten as an invincible worm
        var headPos = head.get<Position>().position;
        var headSize = head.get<Size>().size.X/4;
        if (CircleCircleIntersect(
                headPos,
                headSize,
                entity.get<Position>().position,
                entity.get<Size>().size.X
            ))
        {
            handleWormAteWorm(worm, entity);
        }
    }

    private void wormToSpice(TimeSpan elapsedTime, Entity head, Entity entity)
    {
        var headPos = head.get<Position>().position;
        var headSize = head.get<Size>().size.X/3;
        if (CircleCircleIntersect(
                headPos,
                headSize,
                entity.get<Position>().position,
                entity.get<Size>().size.X
            ))
        {
            handleWormAteSpice(head, entity, elapsedTime);
        }
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
        // There was a collision let everyone know about it
        MessageQueueServer.instance.broadcastMessage(new Collision(head.id, spice.id,
            Collision.CollisionType.HeadToSpice, head.get<Position>()));
        // Remove the spice
        m_entsToRemove.Add(spice);
        // Add power to the worm head
        var headPower = head.get<SpicePower>();
        var spicePower = spice.get<SpicePower>();
        headPower.addPower(spicePower.power);
        // Now we check if the head has grown enough to add a new segment
        if (headPower.power >= POWER_TO_GROW)
        {
            handleWormGrowing(head, headPower);
        }
        m_entsToUpdate.Add(head);
    }

    private void handleWormGrowing(Entity head, SpicePower headPower)
    {
        headPower.resetPower();
        // Add a new segment directly behind the worm head
        var worm = WormMovement.getWormFromHead(head, m_entities);
        var headPos = head.get<Position>().position;
        var segmentPos = new Vector2(headPos.X, headPos.Y);
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
        m_entsToCreate.Add(newSegment);
        foreach (var entity in worm)
        {
            m_entsToUpdate.Add(entity);
        }
        m_entsToUpdate.Add(oldChild);
    }

    private void handleWormAteWorm(List<Entity> worm, Entity otherHead)
    {


        // Check if we hit head on head
        if (otherHead.contains<Head>())
        {
            // There was a collision let everyone know about it
            MessageQueueServer.instance.broadcastMessage(new Collision(worm[0].id, otherHead.id,
                Collision.CollisionType.HeadToHead, worm[0].get<Position>()));
            // We need to compare the sizes of the two worms to see who dies
            List<Entity> otherWorm = WormMovement.getWormFromHead(otherHead, m_entities);
            if (worm.Count > otherWorm.Count)
            {
                handleRemoveWormAndGenerateSpice(otherWorm);
            }
            else
            { 
                handleRemoveWormAndGenerateSpice(worm);
            }
        }
        else // We hit the side of the worm
        {
            // There was a collision let everyone know about it
            MessageQueueServer.instance.broadcastMessage(new Collision(worm[0].id, otherHead.id,
                Collision.CollisionType.HeadToBody, worm[0].get<Position>()));
            // If the worm hit the body, then the worm dies
            handleRemoveWormAndGenerateSpice(worm);
        }
    }
    
    private void handleWormHitWall(List<Entity> worm, Entity wall)
    {
        
        
        // There was a collision let everyone know about it
        MessageQueueServer.instance.broadcastMessage(new Collision(worm[0].id, wall.id,
            Collision.CollisionType.HeadToWall, wall.get<Position>()));
        handleRemoveWormAndGenerateSpice(worm);
    }
    
    private void handleRemoveWormAndGenerateSpice(List<Entity> worm)
    {
        foreach (var entity in worm)
        {
            m_entsToRemove.Add(entity);
        }
        // Generate a new spice
        for (int i = 0; i < worm.Count; i++)
        {
            var spice = DeadWormSpice.create(worm[i].get<Position>().position);
            m_entsToCreate.Add(spice);
        }
    }
    
    public void registerRemoveEntity(Action<uint> removeEntity)
    {
        m_removeEntity = removeEntity;
    }
    
    public void registerAddEntity(Action<Entity> addEntity)
    {
        m_addEntity = addEntity;
    }
    
    private void processNewEntities()
    {
        foreach (var entity in m_entsToCreate)
        {
            m_addEntity(entity);
            MessageQueueServer.instance.broadcastMessage(new NewEntity(entity));
        }
    }
    
    private void processUpdatedEntities(TimeSpan elapsedTime)
    {
        foreach (var entity in m_entsToUpdate)
        {
            MessageQueueServer.instance.broadcastMessage(new UpdateEntity(entity, elapsedTime));
        }
    }
    
    private void processRemovedEntities()
    {
        foreach (var entity in m_entsToRemove)
        {
            m_removeEntity(entity.id);
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(entity.id));
        }
    }
    
}


