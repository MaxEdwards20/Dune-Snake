using Microsoft.Xna.Framework;

namespace Server.Systems;

public class CollisionDetection : Shared.Systems.System
{
    public CollisionDetection() :
        base(
            typeof(Shared.Components.Collision)
        )
    {
    }


    public override void update(TimeSpan elapsedTime)
    {
        throw new NotImplementedException();
    }
    
    // Reference: https://stackoverflow.com/questions/37224912/circle-line-segment-collision
    private static bool CircleLineIntersect(Point pt1, Point pt2, float circleRadius, Vector2 circlePosition)
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
}


