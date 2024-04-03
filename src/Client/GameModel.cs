using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Shared.Components;
using Shared.Entities;

namespace Client;

public class GameModel
{
    private ContentManager m_contentManager;
    private Dictionary<uint, Entity> m_entities;
    private Systems.Network m_systemNetwork;
    private Systems.Camera m_systemCamera;
    private Systems.KeyboardInput m_systemKeyboardInput;
    private Systems.MouseInput m_systemMouseInput;
    private Systems.Interpolation m_systemInterpolation;
    private Systems.Renderer m_systemRenderer;
    private Systems.NameRenderer m_systemNameRenderer;
    private Controls m_controls;
    private GraphicsDeviceManager m_graphics;
    private SpriteFont m_font;

    /// <summary>
    /// This is where everything performs its update.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
        m_systemNetwork.update(elapsedTime, MessageQueueClient.instance.getMessages());
        m_systemKeyboardInput.update(elapsedTime);
        m_systemMouseInput.update(elapsedTime);
        m_systemInterpolation.update(elapsedTime);
        m_systemCamera.update(elapsedTime);
    }

    /// <summary>
    /// Where we render everything
    /// </summary>

    public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
    {
        m_systemRenderer.render(elapsedTime, spriteBatch);
        m_systemNameRenderer.render(elapsedTime, spriteBatch);
    }

    /// <summary>
    /// This is where all game model initialization occurs.  In the case
    /// of this "game', start by initializing the systems and then
    /// loading the art assets.
    /// </summary>
    public bool initialize(ContentManager contentManager, Controls controls, GraphicsDeviceManager graphics)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/menu");
        m_contentManager = contentManager;
        m_entities = new Dictionary<uint, Entity>();
        m_systemInterpolation = new Systems.Interpolation();
        m_systemCamera = new Systems.Camera(new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
        m_systemRenderer = new Systems.Renderer(m_systemCamera, graphics, m_font);
        m_systemNameRenderer = new Systems.NameRenderer(graphics, m_font);
        m_systemNetwork = new Systems.Network();

        m_systemNetwork.registerNewEntityHandler(handleNewEntity);
        m_systemNetwork.registerRemoveEntityHandler(handleRemoveEntity);
        m_controls = controls;

        m_systemKeyboardInput = new Systems.KeyboardInput(new List<Tuple<Shared.Components.Input.Type, Keys>>
        { }, m_controls);
        m_systemMouseInput = new Systems.MouseInput(m_controls);

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
        
        if (message.hasCollision)
        {
            entity.add(new Collision());
        }
        
        // Worm parts
        
        if (message.hasHead)
        {
            entity.add(new Head());
        }
        
        if (message.hasTail)
        {
            entity.add(new Tail());
        }

        if (message.hasParent)
        {
            entity.add(new ParentId(message.parentId));
        }

        if (message.hasChild)
        {
            entity.add(new ChildId(message.childId));
        }

        if (message.hasName)
        {
            entity.add(new Name(message.name));
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
        // NOTE: Update the systems we use here
        m_entities[entity.id] = entity;
        m_systemKeyboardInput.add(entity);
        m_systemMouseInput.add(entity);
        m_systemRenderer.add(entity);
        m_systemNameRenderer.add(entity);
        m_systemNetwork.add(entity);
        m_systemInterpolation.add(entity);
        m_systemCamera.add(entity);
    }

    /// <summary>
    /// All entity lists for the systems must be given a chance to remove
    /// the entity.
    /// </summary>
    private void removeEntity(uint id)
    {
        // NOTE: Update the systems we use here
        m_entities.Remove(id);
        m_systemKeyboardInput.remove(id);
        m_systemMouseInput.remove(id);
        m_systemNetwork.remove(id);
        m_systemRenderer.remove(id);
        m_systemNameRenderer.remove(id);
        m_systemInterpolation.remove(id);
        m_systemCamera.remove(id);
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

