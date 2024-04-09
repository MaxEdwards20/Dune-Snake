using Microsoft.Xna.Framework;
using Shared.Entities;
using Shared.Messages;
using System.Diagnostics;

namespace Server.Systems;

public class SpiceGen : Shared.Systems.System
{
    private Action<Entity>? m_addEntity;
    private readonly int m_mapSize;
    private readonly int m_maxSpice;
    private readonly Random m_rand = new();

    public SpiceGen(int mapSize, int maxSpice) : base(typeof(Shared.Components.SpicePower))
    {
        m_mapSize = mapSize;
        m_maxSpice = maxSpice;
    }

    public override void update(TimeSpan elapsedTime)
    {
        if (m_entities.Count >= m_maxSpice)
            return;

        for (int i = 0; i < m_maxSpice - m_entities.Count; i++)
        {
            int x = m_rand.Next(m_mapSize);
            int y = m_rand.Next(m_mapSize);
            Entity entity = Spice.create(new Vector2(x, y));
            m_addEntity(entity);
            MessageQueueServer.instance.broadcastMessage(new NewEntity(entity));
        }
    }

    public void registerAddEntity(Action<Entity> addEntity)
    {
        m_addEntity = addEntity;
    }
}