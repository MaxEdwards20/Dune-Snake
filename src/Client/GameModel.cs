using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using System;
using System.Collections.Generic;
using Client.Menu;

namespace Client
{
    public class GameModel
    {
        private ContentManager m_contentManager;
        private Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
        private Systems.Network m_systemNetwork = new Systems.Network();
        private Systems.KeyboardInput m_systemKeyboardInput;
        private Systems.Interpolation m_systemInterpolation = new Systems.Interpolation();
        private Systems.Renderer m_systemRenderer = new Systems.Renderer();

        /// <summary>
        /// This is where everything performs its update.
        /// </summary>
        public void update(TimeSpan elapsedTime)
        {
            m_systemNetwork.update(elapsedTime, MessageQueueClient.instance.getMessages());
            m_systemKeyboardInput.update(elapsedTime);
            m_systemInterpolation.update(elapsedTime);
        }
        
        /// <summary>
        /// Where we render everything
        /// </summary>

        public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
        {
            m_systemRenderer.render(elapsedTime, spriteBatch);
        }

        /// <summary>
        /// This is where all game model initialization occurs.  In the case
        /// of this "game', start by initializing the systems and then
        /// loading the art assets.
        /// </summary>
        public bool initialize(ContentManager contentManager)
        {
            m_contentManager = contentManager;
            m_systemNetwork.registerNewEntityHandler(handleNewEntity);
            m_systemNetwork.registerRemoveEntityHandler(handleRemoveEntity);

            m_systemKeyboardInput = new Systems.KeyboardInput(new List<Tuple<Shared.Components.Input.Type, Keys>>
            { });

            return true;
        }
        
        public void shutdown()
        {

        }

        /// <summary>
        /// Based upon an Entity received from the server, create the
        /// entity at the client.
        /// </summary>
        private Entity createEntity(Shared.Messages.NewEntity message)
        {
            Entity entity = new Entity(message.id);
            

            if (message.hasAppearance)
            {
                Texture2D texture = m_contentManager.Load<Texture2D>(message.texture);
                entity.add(new Components.Sprite(texture));
            }

            if (message.hasPosition)
            {
                entity.add(new Shared.Components.Position(message.position, message.orientation));
            }

            if (message.hasSize)
            {
                entity.add(new Shared.Components.Size(message.size));
            }

            if (message.hasMovement)
            {
                entity.add(new Shared.Components.Movement(message.moveRate, message.rotateRate));
            }

            if (message.hasInput)
            {
                entity.add(new Shared.Components.Input(message.inputs));
            }

            return entity;
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
            m_systemKeyboardInput.add(entity);
            m_systemRenderer.add(entity);
            m_systemNetwork.add(entity);
            m_systemInterpolation.add(entity);
        }

        /// <summary>
        /// All entity lists for the systems must be given a chance to remove
        /// the entity.
        /// </summary>
        private void removeEntity(uint id)
        {
            m_entities.Remove(id);

            m_systemKeyboardInput.remove(id);
            m_systemNetwork.remove(id);
            m_systemRenderer.remove(id);
            m_systemInterpolation.remove(id);
        }

        private void handleNewEntity(Shared.Messages.NewEntity message)
        {
            Entity entity = createEntity(message);
            addEntity(entity);
        }

        /// <summary>
        /// Handler for the RemoveEntity message.  It removes the entity from
        /// the client game model (that's us!).
        /// </summary>
        private void handleRemoveEntity(Shared.Messages.RemoveEntity message)
        {
            removeEntity(message.id);
        }
    }
}
