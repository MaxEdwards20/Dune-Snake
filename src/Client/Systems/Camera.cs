using Microsoft.Xna.Framework;
using Shared.Entities;
using System;
using System.Diagnostics;
using Shared.Components;

namespace Client.Systems;

public class Camera : Shared.Systems.System
{
    private Rectangle m_viewport = new();
    public Rectangle Viewport { get { return m_viewport; } }
    private const float m_maxScale = 0.6f;
    private const float m_minScale = 0.3f;
    private const int m_zoomInTime = 1000;
    private const int m_zoomOutTime = 15000;
    private float m_currentZoom = m_minScale;
    public float Zoom { get { return m_currentZoom; } }
    private PlayerData m_playerData;
    private bool m_playing = false;
    private float m_bezierCurveMS = 0;
    private float m_interpTime;

    public Camera(Vector2 viewportSize, PlayerData playerData) : base(typeof(Input))
    {
        m_viewport.Size = viewportSize.ToPoint();
        m_playerData = playerData;
    }

    public override void update(TimeSpan elapsedTime)
    {
        if (m_entities.Count > 1)
            throw new Exception("Got an invalid number of players on the client side.");

        if (m_entities.Count < 1)
        {
            if (m_playing)
            {
                m_playing = false;
                m_interpTime = m_zoomOutTime;
                m_bezierCurveMS = m_interpTime;
            }
            BezierZoom(elapsedTime, zoomIn: false);
            return;
        }

        if (!m_playing)
        {
            m_playing = true;
            m_interpTime = m_zoomInTime;
            m_bezierCurveMS = 0;
        }
        BezierZoom(elapsedTime, zoomIn: true);

        foreach (var e in m_entities.Values)
        {
            if (e.contains<Name>() && e.get<Name>().name == m_playerData.playerName)
            {
                Entity player = e;
                Vector2 pos = player.get<Position>().position;

                // TODO: Change zoom depending on player size

                m_viewport.Location = pos.ToPoint();
                return;
            }
        }
    }

    private void BezierZoom(TimeSpan elapsedTime, bool zoomIn = true)
    {
        int direction = zoomIn ? 1 : -1;

        m_bezierCurveMS += (float)(direction * elapsedTime.TotalMilliseconds);
        m_bezierCurveMS = MathHelper.Clamp(m_bezierCurveMS, 0, m_interpTime);
        float t = MathHelper.Clamp(m_bezierCurveMS / m_interpTime, 0, 1);

        // Got curves from https://cubic-bezier.com/
        Vector2 p1 = new(0, 1.03f); // Control point 1
        Vector2 p2 = new(0, 0.85f); // Control point 2
        if (!zoomIn)
        {
            p1 = new(1, 0); // Control point 1
            p2 = new(1, 0); // Control point 2
        }

        Vector2 position = BezierCurve.GetPoint(p1, p2, t);

        // Result of beziercurve Y is between 0 and 1, transform back to our scale
        float scale = m_maxScale - m_minScale;
        float translation = m_minScale;
        m_currentZoom = position.Y * scale + translation;
    }
}


public static class BezierCurve
{
    public static Vector2 GetPoint(Vector2 p1, Vector2 p2, float t)
    {
        return GetPoint(Vector2.Zero, p1, p2, Vector2.One, t);
    }


    // Get a point on the Bezier curve using De Casteljau's algorithm
    public static Vector2 GetPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        // interpolate between points
        Vector2 q0 = Lerp(p0, p1, t);
        Vector2 q1 = Lerp(p1, p2, t);
        Vector2 q2 = Lerp(p2, p3, t);

        // interpolate between the new points
        Vector2 r0 = Lerp(q0, q1, t);
        Vector2 r1 = Lerp(q1, q2, t);

        // interpolate between the last two points
        return Lerp(r0, r1, t);
    }

    // Linear interpolation between two vectors
    private static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }
}

