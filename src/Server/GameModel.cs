
using Microsoft.Xna.Framework;
using Server.Systems;
using Shared.Components;
using Shared.Components.Appearance;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;

namespace Server;

public class GameModel
{
    private readonly HashSet<int> m_clients = new();
    private readonly Dictionary<uint, Entity> m_entities = new();
    private readonly Dictionary<int, uint> m_clientToEntityId = new();
    private readonly WormMovement m_systemWormMovement = new();
    private readonly CollisionDetection m_systemCollisionDetection = new();
    private readonly GrowthHandler m_SystemGrowthHandler = new();
    private readonly Network m_systemNetwork = new();
    private readonly SpiceGen m_systemSpiceGen = new(mapSize - 200, 300);
    private const int mapSize = 3000;

    /// <summary>
    /// This is where the server-side simulation takes place.  Messages
    /// from the network are processed and then any necessary client
    /// updates are sent out.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
        m_systemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
        m_systemCollisionDetection.update(elapsedTime);
        m_SystemGrowthHandler.update(elapsedTime);
        m_systemWormMovement.update(elapsedTime);
        m_systemSpiceGen.update(elapsedTime);
    }

    /// <summary>
    /// Setup notifications for when new clients connect.
    /// </summary>
    public bool initialize()
    {
        generateWalls();
        m_systemNetwork.registerJoinHandler(handleJoin);
        m_systemNetwork.registerDisconnectHandler(handleDisconnect);
        MessageQueueServer.instance.registerConnectHandler(handleConnect);
        m_systemCollisionDetection.registerRemoveEntity(removeEntity);
        m_systemCollisionDetection.registerAddEntity(addEntity);
        m_systemSpiceGen.registerAddEntity(addEntity);
        return true;
    }

    /// <summary>
    /// Give everything a chance to gracefully shutdown.
    /// </summary>
    public void shutdown() { }

    /// <summary>
    /// Upon connection of a new client, create a player entity and
    /// send that info back to the client, along with adding it to
    /// the server simulation.
    /// </summary>
    private void handleConnect(int clientId)
    {
        m_clients.Add(clientId);

        MessageQueueServer.instance.sendMessage(clientId, new ConnectAck());
    }

    /// <summary>
    /// When a client disconnects, need to tell all the other clients
    /// of the disconnect.
    /// </summary>
    /// <param name="clientId"></param>
    private void handleDisconnect(int clientId)
    {
        m_clients.Remove(clientId);
        Message message = new RemoveEntity(m_clientToEntityId[clientId]);
        MessageQueueServer.instance.broadcastMessage(message);
        removeEntity(m_clientToEntityId[clientId]);
        m_clientToEntityId.Remove(clientId);
    }

    /// <summary>
    /// As entities are added to the game model, they are run by the systems
    /// to see if they are interested in knowing about them during their
    /// updates.
    /// </summary>
    private void addEntity(Entity entity)
    {
        if (entity == null)
            return;

        m_entities[entity.id] = entity;
        m_systemNetwork.add(entity);
        m_systemCollisionDetection.add(entity);
        m_SystemGrowthHandler.add(entity);
        m_systemWormMovement.add(entity);
        m_systemSpiceGen.add(entity);
    }

    /// <summary>
    /// All entity lists for the systems must be given a chance to remove
    /// the entity.
    /// </summary>
    private void removeEntity(uint id)
    {
        m_entities.Remove(id);
        m_systemNetwork.remove(id);
        m_systemCollisionDetection.remove(id);
        m_SystemGrowthHandler.remove(id);
        m_systemWormMovement.remove(id);
        m_systemSpiceGen.remove(id);
    }

    /// <summary>
    /// For the indicated client, sends messages for all other entities
    /// currently in the game simulation.
    /// </summary>
    private void reportAllEntities(int clientId)
    {
        foreach (var item in m_entities)
            MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.NewEntity(item.Value));
    }

    /// <summary>
    /// Handler for the Join message.  It gets a player entity created,
    /// added to the server game model, and notifies the requesting client
    /// of the player.
    /// </summary>
    private void handleJoin(int clientId, Shared.Messages.Message message)
    {
        // Create a default name for the player
        var joinMessage = (Join)message;
        string name = joinMessage.name;
        // Step 1: Tell the newly connected player about all other entities
        reportAllEntities(clientId);

        // Step 2: Create a new wormHead for the newly joined player and sent it
        //         to the newly joined client
        createNewWorm(clientId, name);
    }

    private void generateWalls()
    {
        // We want to create wall entities around the entire map. 5000x5000 is the size of the map
        // We'll create a wall every 100 units
        var wallSize = 100;
        for (int i = 0; i < mapSize / 100; i++)
        {
            // Top wall
            Entity wall = Shared.Entities.Wall.create(new Vector2(i * wallSize, 0), wallSize);
            addEntity(wall);

            // Bottom wall
            wall = Shared.Entities.Wall.create(new Vector2(i * wallSize, mapSize - wallSize), wallSize);
            addEntity(wall);

            // Left wall
            wall = Shared.Entities.Wall.create(new Vector2(0, i * wallSize), wallSize);
            addEntity(wall);

            // Right wall
            wall = Shared.Entities.Wall.create(new Vector2(mapSize - wallSize, i * wallSize), wallSize);
            addEntity(wall);
        }
    }

    private void createNewWorm(int clientId, string name)
    {
        var headStartLocation = getLeastDenseStartLocation();
        var segmentStartLocation = new Vector2(headStartLocation.X - 75, headStartLocation.Y);
        var rotationRate = (float) Math.PI / 1000;
        var moveRate = 0.3f;
        var headSize = 100;
        var bodySize = 80;

        // Create the head
        Entity? segment = WormHead.create(headStartLocation, name);
        // Create X number of body segments
        var parent = segment;
        var numToCreate = 5;
        for (int i = 0; i < numToCreate; i++)
        {
            segment = WormSegment.create(segmentStartLocation,  parent.id);
            if (i == numToCreate - 1)
            {
                segment = WormTail.create(segmentStartLocation,  parent.id);
            }
            parent.add(new ChildId(segment.id));
            addEntity(parent);
            m_clientToEntityId[clientId] = parent.id;
            MessageQueueServer.instance.sendMessage(clientId, new NewEntity(parent));
            segmentStartLocation = new Vector2(segmentStartLocation.X - 50, segmentStartLocation.Y);
            parent = segment;
        }
        addEntity(segment);
        m_clientToEntityId[clientId] = segment.id;
        MessageQueueServer.instance.sendMessage(clientId, new NewEntity(segment));

        // Step 4: Let all other clients know about this new player 
        while (segment != null)
        {
            // Don't need to send the input component to other clients
            if (segment.contains<Shared.Components.Input>())
            {
                segment.remove<Shared.Components.Input>();
            }
            // Send to each of the other clients
            foreach (int otherId in m_clients)
            {
                if (otherId != clientId)
                {
                    var message = new NewEntity(segment);
                    MessageQueueServer.instance.sendMessage(otherId, message);
                }
            }
            // Move up the linked list
            if (segment.contains<ParentId>())
            {
                segment = m_entities[segment.get<ParentId>().id];
            }
            else
            {
                segment = null;
            }
        }
    }

    private Vector2 getLeastDenseStartLocation()
    {
        // We want to start the player in the least dense area of the screen
        // For now, we'll just start them randomly generated location
        Random random = new Random();
        var lowerBound = (int)(300);
        var upperBound = (int)(mapSize - 300);
        return new Vector2(random.Next(lowerBound, upperBound), random.Next(lowerBound, upperBound));
    }
}

