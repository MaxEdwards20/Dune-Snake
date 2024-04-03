
using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;
using Shared.Entities;
using Shared.Messages;

namespace Server
{
    public class GameModel
    {
        private HashSet<int> m_clients = new HashSet<int>();
        private Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
        private Dictionary<int, uint> m_clientToEntityId = new Dictionary<int, uint>();

        Systems.Network m_systemNetwork = new Server.Systems.Network();

        /// <summary>
        /// This is where the server-side simulation takes place.  Messages
        /// from the network are processed and then any necessary client
        /// updates are sent out.
        /// </summary>
        public void update(TimeSpan elapsedTime)
        {
            m_systemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
        }

        /// <summary>
        /// Setup notifications for when new clients connect.
        /// </summary>
        public bool initialize()
        {
            m_systemNetwork.registerJoinHandler(handleJoin);
            m_systemNetwork.registerDisconnectHandler(handleDisconnect);

            MessageQueueServer.instance.registerConnectHandler(handleConnect);

            return true;
        }

        /// <summary>
        /// Give everything a chance to gracefully shutdown.
        /// </summary>
        public void shutdown()
        {

        }

        /// <summary>
        /// Upon connection of a new client, create a player entity and
        /// send that info back to the client, along with adding it to
        /// the server simulation.
        /// </summary>
        private void handleConnect(int clientId)
        {
            m_clients.Add(clientId);

            MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.ConnectAck());
        }

        /// <summary>
        /// When a client disconnects, need to tell all the other clients
        /// of the disconnect.
        /// </summary>
        /// <param name="clientId"></param>
        private void handleDisconnect(int clientId)
        {
            m_clients.Remove(clientId);

            Message message = new Shared.Messages.RemoveEntity(m_clientToEntityId[clientId]);
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
            {
                return;
            }

            m_entities[entity.id] = entity;
            m_systemNetwork.add(entity);
        }

        /// <summary>
        /// All entity lists for the systems must be given a chance to remove
        /// the entity.
        /// </summary>
        private void removeEntity(uint id)
        {
            m_entities.Remove(id);
            m_systemNetwork.remove(id);
        }

        /// <summary>
        /// For the indicated client, sends messages for all other entities
        /// currently in the game simulation.
        /// </summary>
        private void reportAllEntities(int clientId)
        {
            foreach (var item in m_entities)
            {
                MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.NewEntity(item.Value));
            }
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
            string name = "Player" + clientId;
            // Step 1: Tell the newly connected player about all other entities
            reportAllEntities(clientId);

            // Step 2: Create a new wormHead for the newly joined player and sent it
            //         to the newly joined client
            createNewWorm(clientId, name);
        }

        private void createNewWorm(int clientId, string name)
        {
            var startLocation = new Vector2(200, 200);
            var rotationRate = (float) Math.PI / 1000;
            var moveRate = 0.1f;
            var headSize = 100;
            var bodySize = 80;
            
            // Create the head
            Entity player = WormHead.create(startLocation, 100, moveRate, rotationRate, name);
            
            // Create a body segment
            Entity segment = WormSegment.create( new Vector2(startLocation.X + 75, startLocation.Y - 20)  , bodySize, moveRate, rotationRate, player.id);
            player.add(new ChildId(segment.id));
            
            // Create a tail segment
            Entity tail = WormTail.create(new Vector2(startLocation.X + 130, startLocation.Y), bodySize, moveRate, rotationRate, segment.id);
            segment.add(new ChildId(tail.id));
            
            addEntity(player);
            addEntity(segment);
            addEntity(tail);
            
            m_clientToEntityId[clientId] = player.id;
            m_clientToEntityId[clientId] = segment.id;
            m_clientToEntityId[clientId] = tail.id;

            // Step 3: Send the new player entity to the newly joined client
            MessageQueueServer.instance.sendMessage(clientId, new NewEntity(player));
            MessageQueueServer.instance.sendMessage(clientId, new NewEntity(segment));
            MessageQueueServer.instance.sendMessage(clientId, new NewEntity(tail));

            // Step 4: Let all other clients know about this new player entity
            // Remove components not needed for "other" players
            player.remove<Shared.Components.Input>();
            Message message = new NewEntity(player);
            foreach (int otherId in m_clients)
            {
                if (otherId != clientId)
                {
                    MessageQueueServer.instance.sendMessage(otherId, message);
                }
            }
            
        }
    }
}
