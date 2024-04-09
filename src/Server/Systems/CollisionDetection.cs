using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;

namespace Server.Systems;

public class CollisionDetection : Shared.Systems.System
{
    private System.Action<uint> m_removeEntity;
    
    public CollisionDetection() :
        base(
            typeof(Shared.Components.Collision), typeof(Shared.Components.Position), typeof(Shared.Components.Size)
        )
    {
    }
    
    public override void update(TimeSpan elapsedTime)
    {
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
                
                if (entity.contains<SpicePower>() )
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
                    if (CircleLineIntersect(
                            entity.get<Position>().position,
                            entity.get<Position>().position + new Vector2(entity.get<Size>().size.X, 0),
                            head.get<Size>().size.X,
                            head.get<Position>().position
                        ) || CircleLineIntersect(
                            entity.get<Position>().position,
                            entity.get<Position>().position + new Vector2(0, entity.get<Size>().size.Y),
                            head.get<Size>().size.X,
                            head.get<Position>().position
                        ))
                    {
                        handleWormHitWall(worm);
                    }
                }
            }
        }
    }
    
    // Reference: https://stackoverflow.com/questions/37224912/circle-line-segment-collision
    private static bool CircleLineIntersect(Vector2 pt1, Vector2 pt2, float circleRadius, Vector2 circlePosition)
    {
        Vector2 v1 = new Vector2((float)(pt2.X - pt1.X), (float)(pt2.X - pt1.X));
        Vector2 v2 = new Vector2((float) pt1.X - circlePosition.X, (float)(pt1.X - circlePosition.Y));
        float b = -2 * (v1.X * v2.X + v1.Y * v2.Y);
        float c = 2 * (v1.X * v1.X + v1.Y * v1.Y);
        float d = (float)Math.Sqrt(b * b - 2 * c * (v2.X * v2.X + v2.Y * v2.Y - circleRadius * circleRadius));
        if (float.IsNaN(d)) // no intercept
        {
            return false;
        }
        // These represent the unit distance of point one and two on the line
        float u1 = (b - d) / c;
        float u2 = (b + d) / c;
        if (u1 <= 1 && u1 >= 0) // If point on the line segment
        {
            return true;
        }
        if (u2 <= 1 && u2 >= 0) // If point on the line segment
        {
            return true;
        }
        return false;
    }
    
    private static bool CircleCircleIntersect(Vector2 position1, float radius1, Vector2 position2, float radius2)
    {
        return Vector2.Distance(position1, position2) < radius1 + radius2;
    }
    
    private void handleWormAteSpice(Entity head, Entity spice, TimeSpan elapsedTime)
    {
        // Remove the spice
        MessageQueueServer.instance.broadcastMessageWithLastId(new RemoveEntity(spice.id));
        // Add power to the worm head
        var headPower = head.get<SpicePower>();
        var spicePower = spice.get<SpicePower>();
        headPower.addPower(spicePower.power);
        MessageQueueServer.instance.broadcastMessageWithLastId(new UpdateEntity(head, elapsedTime));
    }
    
    private void handleWormAteWorm(List<Entity> worm, Entity otherHead)
    {
        // Check if we hit head on
        if (otherHead.contains<Head>())
        {
            // We need to compare the sizes of the two worms to see who dies
            List<Entity> otherWorm = WormMovement.getWormFromHead(otherHead, m_entities);
            if (worm.Count > otherWorm.Count)
            {
                removeWorm(otherWorm);
            }
            else
            {
                removeWorm(worm);
            }
        }
        else // We hit the side of the worm
        {
            // If the worm hit the body, then the worm dies
            removeWorm(worm);
        }
    }
    
    private void handleWormHitWall(List<Entity> worm)
    {
        removeWorm(worm);
    }

    private void removeWorm(List<Entity> worm)
    {
        foreach (var entity in worm)
        {
            
            MessageQueueServer.instance.broadcastMessageWithLastId(new RemoveEntity(entity.id));
            m_removeEntity(entity.id);
        }
        // TODO: Add new entities to the world where the body was
        
    }

    public void registerRemoveEntity(Action<uint> removeEntity)
    {
        m_removeEntity = removeEntity;
    }
}


