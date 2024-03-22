using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Reflection.Metadata;

namespace CS5410.Objects
{ 

class CollisionDetector
{
    public static bool CheckCollision(Vector2 landerPosition, float landerRadius, List<TPoint> terrainLine)
    {
        for (int i = 0; i < terrainLine.Count - 1; i++)
        {
            TPoint lineStart = terrainLine[i];
            TPoint lineEnd = terrainLine[i + 1];

            if (CircleLineIntersect(lineStart, lineEnd, landerRadius, landerPosition))
            {
                return true; // Collision detected
            }
        }

        return false; // No collision
    }

    public static bool IsCollisionOnSafeZone(Vector2 landerPosition, float landerRadius, List<TPoint> safeZonePoints)
    {
        for (int i = 0; i < safeZonePoints.Count - 1; i+= 2)
        {
            TPoint lineStart = safeZonePoints[i];
            TPoint lineEnd = safeZonePoints[i + 1];

            if (CircleLineIntersect(lineStart, lineEnd, landerRadius, landerPosition))
            {
                return true; // Collision detected
            }
        }

        return false; // No collision
    }


    // Reference: https://stackoverflow.com/questions/37224912/circle-line-segment-collision
private static bool CircleLineIntersect(TPoint pt1, TPoint pt2, float circleRadius, Vector2 circlePosition)
{
    Vector2 v1 = new Vector2((float)(pt2.x - pt1.x), (float)(pt2.y - pt1.y));
    Vector2 v2 = new Vector2((float) pt1.x - circlePosition.X, (float)(pt1.y - circlePosition.Y));
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
}
