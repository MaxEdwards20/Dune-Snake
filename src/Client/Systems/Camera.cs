using Microsoft.Xna.Framework;
using Shared.Entities;
using System;
using System.Diagnostics;
using System.Linq;
using Shared.Components;

namespace Client.Systems;

public class Camera : Shared.Systems.System
{
    private Rectangle m_viewport = new();
    public Rectangle Viewport { get { return m_viewport; } }
    private float m_zoom = 0.6f;
    public float Zoom { get { return m_zoom; } }
    private PlayerData m_playerData;

    public Camera(Vector2 viewportSize, PlayerData playerData) : base(typeof(Shared.Components.Input))
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
            Debug.WriteLine("No Player Found");
            return;
        }

        
        foreach (var e in m_entities.Values)
        {
            if (e.contains<Name>() && e.get<Name>().name == m_playerData.playerName)
            {
                Entity player = e;
                Vector2 pos = player.get<Shared.Components.Position>().position;
                Vector2 size = player.get<Shared.Components.Size>().size;

                m_viewport.Location = pos.ToPoint();
                return;
            }
        }
        
        // TODO: Change zoom depending on factors (player size, player death, etc.)
    }
}