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
    private float m_zoom = 1.0f;
    public float Zoom { get { return m_zoom; } }

    public Camera(Vector2 viewportSize) :
        base(
            typeof(Shared.Components.Position),
            typeof(Shared.Components.Movement),
            typeof(Shared.Components.Input),
            typeof(Shared.Components.Size)
        )
    { m_viewport.Size = viewportSize.ToPoint(); }

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
        Vector2 pos = player.get<Shared.Components.Position>().position;
        Vector2 size = player.get<Shared.Components.Size>().size;

        m_viewport.Location = pos.ToPoint() - (size / 2).ToPoint();

        // TODO: Change zoom depending on factors (player size, player death, etc.)
    }
}