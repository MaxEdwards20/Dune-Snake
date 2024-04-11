using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;

namespace Server.Systems;

public class Network : Shared.Systems.System
{
    public delegate void Handler(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message);
    public delegate void JoinHandler(int clientId, Shared.Messages.Message message);
    public delegate void DisconnectHandler(int clientId);
    public delegate void InputHandler(Entity entity, Shared.Components.Input.Type type, TimeSpan elapsedTime);

    private Dictionary<Shared.Messages.Type, Handler> m_commandMap = new Dictionary<Shared.Messages.Type, Handler>();
    private JoinHandler m_joinHandler;
    private DisconnectHandler m_disconnectHandler;
    private HashSet<uint> m_reportThese = new HashSet<uint>();
    private TimeSpan m_lastGlobalUpdateTime = new TimeSpan(m_globalUpdateFrequency);
    private static int m_globalUpdateFrequency = 300;

    /// <summary>
    /// Primary activity in the constructor is to setup the command map
    /// that maps from message types to their handlers.
    /// </summary>
    public Network() :
        base(
            typeof(Shared.Components.Movement),
            typeof(Shared.Components.Position)
        )
    {
        // Register our own join handler
        registerHandler(Shared.Messages.Type.Join, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
        {
            if (m_joinHandler != null)
            {
                m_joinHandler(clientId, message);
            }
        });

        // Register our own disconnect handler
        registerHandler(Shared.Messages.Type.Disconnect, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
        {
            if (m_disconnectHandler != null)
            {
                m_disconnectHandler(clientId);
            }
        });

        // Register our own input handler
        registerHandler(Shared.Messages.Type.Input, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
        {
            handleInput((Shared.Messages.Input)message);
        });
    }

    // Have to implement this because it is abstract in the base class
    public override void update(TimeSpan elapsedTime)
    {
        m_lastGlobalUpdateTime -= elapsedTime;
        if (m_lastGlobalUpdateTime.TotalMilliseconds < 0)
        {
            Console.WriteLine("Global Worm Location Update");
            m_lastGlobalUpdateTime = new TimeSpan(m_globalUpdateFrequency);
            foreach (var entity in m_entities.Values)
            {
                if (entity.contains<Shared.Components.Worm>())
                {
                    m_reportThese.Add(entity.id);
                }
            }
        }
    }

    /// <summary>
    /// Have our own version of update, because we need a list of messages to work with, and
    /// messages aren't entities.
    /// </summary>
    public void update(TimeSpan elapsedTime, Queue<Tuple<int, Message>> messages)
    {
        if (messages != null)
        {
            while (messages.Count > 0)
            {
                var message = messages.Dequeue();
                if (m_commandMap.ContainsKey(message.Item2.type))
                {
                    m_commandMap[message.Item2.type](message.Item1, elapsedTime, message.Item2);
                }
            }
        }

        // Send updated game state updates back out to connected clients
        updateClients(elapsedTime);
    }

    public void registerJoinHandler(JoinHandler handler)
    {
        m_joinHandler = handler;
    }

    public void registerDisconnectHandler(DisconnectHandler handler)
    {
        m_disconnectHandler = handler;
    }

    private void registerHandler(Shared.Messages.Type type, Handler handler)
    {
        m_commandMap[type] = handler;
    }

    /// <summary>
    /// Handler for the Input message.  This simply passes the responsibility
    /// to the registered input handler.
    /// </summary>
    /// <param name="message"></param>
    private void handleInput(Shared.Messages.Input message)
    {
        var entity = m_entities[message.entityId];
        var worm = WormMovement.getWormFromHead(entity, m_entities);
        var update = false;
        foreach (var input in message.inputs)
        {
            switch (input)
            {
                case Shared.Components.Input.Type.PointLeft:
                    Shared.Systems.WormMovement.left(worm, message.elapsedTime);
                    update = true;
                    break;
                case Shared.Components.Input.Type.PointRight:
                    Shared.Systems.WormMovement.right(worm, message.elapsedTime);
                    update = true;
                    break;
                case Shared.Components.Input.Type.PointUp:
                    Shared.Systems.WormMovement.up(worm);
                    update = true;
                    break;
                case Shared.Components.Input.Type.PointDown:
                    Shared.Systems.WormMovement.down(worm);
                    update = true;
                    break;
                case Shared.Components.Input.Type.PointUpLeft:
                    Shared.Systems.WormMovement.upLeft(worm);
                    update = true;
                    break;
                case Shared.Components.Input.Type.PointUpRight:
                    Shared.Systems.WormMovement.upRight(worm);
                    update = true;
                    break;  
                case Shared.Components.Input.Type.PointDownLeft:
                    Shared.Systems.WormMovement.downLeft(worm);
                    update = true;
                    break;
                case Shared.Components.Input.Type.PointDownRight:
                    Shared.Systems.WormMovement.downRight(worm);
                    update = true;
                    break;
            }
            MessageQueueServer.instance.broadcastMessage(new NewAnchorPoint(worm[0].get<Position>(), worm[0].id));
        }
        if (update)
        {
            foreach (var e in worm)
            {
                m_reportThese.Add(e.id);
            }
        }
    }

    /// <summary>
    /// For the entities that have updates, send those updates to all
    /// connected clients.
    /// </summary>
    private void updateClients(TimeSpan elapsedTime)
    {
        foreach (var entityId in m_reportThese)
        {
            var entity = m_entities[entityId];
            var message = new Shared.Messages.UpdateEntity(entity, elapsedTime);
            MessageQueueServer.instance.broadcastMessageWithLastId(message);
        }

        m_reportThese.Clear();
    }
}
