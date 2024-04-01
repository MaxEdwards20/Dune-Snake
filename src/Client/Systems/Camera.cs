using Microsoft.Xna.Framework;
using Shared.Entities;
using System;
using System.Diagnostics;
using System.Linq;

namespace Client.Systems;

public class Camera : Shared.Systems.System
{
    private Rectangle m_viewport = new();
    public Rectangle Viewport { get { return m_viewport; } }

    public Camera() :
        base(
            typeof(Shared.Components.Position),
            typeof(Shared.Components.Movement),
            typeof(Shared.Components.Input)
        )
    { }

    public override void update(TimeSpan elapsedTime)
    {
        if (m_entities.Count > 1)
            throw new Exception("Got an invalid number of players on the client side.");

        if (m_entities.Count < 1)
        {
            Debug.WriteLine("No Player Found");
            return;
        }

        Entity player = m_entities.Values.ToArray()[0];
        Point pos = player.get<Shared.Components.Position>().position.ToPoint();

        m_viewport.Location = pos;
    }
}