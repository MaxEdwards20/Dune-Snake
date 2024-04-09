using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;

namespace Server.Systems;

public class CollisionDetection : Shared.Systems.System
{
    public CollisionDetection() :
        base(
            typeof(Shared.Components.Collision), typeof(Shared.Components.Position), typeof(Shared.Components.Size)
        )
    {
    }
    
    public override void update(TimeSpan elapsedTime)
    {
        // If we collide with food, we need to remove the food and grow the worm
        // If we collide with a worm, we need to remove the worm and end the game for that client
        // If we collide with a wall, we need to remove the worm and end the game for that client
        
        // We know that the heads of the worms are the only things that may be colliding with something. So we just need to check the heads of the worms against everything else.
        
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
                
                if (entity.contains<SpicePower>() || entity.contains<Worm>())
                {
                    if (CircleCircleIntersect(
                        head.get<Position>().position,
                        head.get<Size>().size.X,
                        entity.get<Position>().position,
                        entity.get<Size>().size.X
                    ))
                    {
                        MessageQueueServer.instance.broadcastMessage(new Shared.Messages.Collision(head.id, entity.id));
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
                        MessageQueueServer.instance.broadcastMessage(new Shared.Messages.Collision(head.id, entity.id));
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
}


