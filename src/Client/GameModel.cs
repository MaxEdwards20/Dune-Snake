using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Client.Components;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;
using Microsoft.Xna.Framework.Audio;
using CS5410;

namespace Client;

public class GameModel
{
    private ContentManager m_contentManager;
    private Dictionary<uint, Entity> m_entities;
    private Systems.Network m_systemNetwork;
    private Systems.Camera m_systemCamera;
    private Systems.KeyboardInput m_systemKeyboardInput;
    private Systems.Interpolation m_systemInterpolation;
    private Systems.Renderer m_renderer;
    private Shared.Systems.WormMovement m_systemWormMovement;
    private Shared.Systems.GrowthHandler m_systemGrowthHandler;
    private Controls m_controls;
    private GraphicsDeviceManager m_graphics;
    private SpriteFont m_font;
    private SpriteFont m_fontSmall;
    private Texture2D m_sand;
    private String m_playerName;
    private PlayerData m_playerData;
    private SoundEffect m_deathSound;
    private SoundEffect m_eatSpiceSound;
    private SoundEffectInstance m_deathSoundInstance;
    private SoundEffectInstance m_eatSpiceSoundInstance;
    private ParticleSystem deathParticleSystem;
    private ParticleSystem eatParticleSystem;
    private ParticleSystemRenderer deathRenderer;
    private ParticleSystemRenderer eatRenderer;
    private bool collisionIsOn;
    private bool spiceEaten;

    public GameModel(StringBuilder playerName)
    {
        m_playerName = playerName.ToString();
    }

    /// <summary>
    /// This is where everything performs its update.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
        m_systemNetwork.update(elapsedTime, MessageQueueClient.instance.getMessages());
        m_systemKeyboardInput.update(elapsedTime);
        m_systemGrowthHandler.update(elapsedTime);
        m_systemWormMovement.update(elapsedTime);
        m_systemInterpolation.update(elapsedTime);
        m_systemCamera.update(elapsedTime);
        // m_systemScore.update(elapsedTime); // TODO

        GameTime gameTime = new GameTime(totalGameTime: TimeSpan.Zero, elapsedGameTime: elapsedTime);


        if (eatParticleSystem != null)
        {
            eatParticleSystem.update(gameTime);
        }

        if (deathParticleSystem != null)
        {
            deathParticleSystem.update(gameTime);
        }

    }

    /// <summary>
    /// Where we render everything
    /// </summary>

    public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

        eatRenderer.draw(spriteBatch, eatParticleSystem);
        deathRenderer.draw(spriteBatch, deathParticleSystem);


        spriteBatch.End();
        
        m_renderer.render(elapsedTime, spriteBatch);
    }

    /// <summary>
    /// This is where all game model initialization occurs.  In the case
    /// of this "game', start by initializing the systems and then
    /// loading the art assets.
    /// </summary>
    public bool initialize(ContentManager contentManager, Controls controls, GraphicsDeviceManager graphics)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/menu");
        m_fontSmall = contentManager.Load<SpriteFont>("Fonts/roboto-small");
        m_sand = contentManager.Load<Texture2D>("Textures/SandTile");

        m_deathSound = contentManager.Load<SoundEffect>("Audio/death");
        m_eatSpiceSound = contentManager.Load<SoundEffect>("Audio/eatSpice");

        m_deathSoundInstance = m_deathSound.CreateInstance();
        m_eatSpiceSoundInstance = m_eatSpiceSound.CreateInstance();

        eatRenderer = new ParticleSystemRenderer("Textures/particle");
        deathRenderer = new ParticleSystemRenderer("Textures/particle");

        eatParticleSystem = new ParticleSystem(new Vector2(0, 0), 2, 1, 0.2f, 0.1f, 300, 150);
        deathParticleSystem = new ParticleSystem(new Vector2(0, 0), 4, 2, 0.5f, 0.25f, 1000, 500);

        eatRenderer.LoadContent(contentManager);
        deathRenderer.LoadContent(contentManager);

        m_contentManager = contentManager;
        m_entities = new Dictionary<uint, Entity>();
        m_systemInterpolation = new Systems.Interpolation();
        m_systemCamera = new Systems.Camera(new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));

        m_renderer = new Systems.Renderer(m_systemCamera, graphics, m_font, m_fontSmall, m_sand);
        m_systemGrowthHandler = new Shared.Systems.GrowthHandler();
        m_systemWormMovement = new Shared.Systems.WormMovement();
        m_systemNetwork = new Systems.Network(m_playerName);

        m_systemNetwork.registerNewEntityHandler(handleNewEntity);
        m_systemNetwork.registerRemoveEntityHandler(handleRemoveEntity);
        m_systemNetwork.registerCollisionHandler(handleCollision);
        m_systemNetwork.registerNewAnchorPointHandler(handleNewAnchorPoint);
        m_controls = controls;



        m_systemKeyboardInput = new Systems.KeyboardInput(new List<Tuple<Shared.Components.Input.Type, Keys>>
        { }, m_controls);

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
            entity.add(new Collidable());
        }
        
        if (message.hasWall)
        {
            entity.add(new Shared.Components.Wall());
        }

        // Worm parts
        
        if (message.hasWorm)
        {
            entity.add(new Worm());
            entity.add(new AnchorQueue()); // We implicitly need this because every worm part has it
        }

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
        
        if (message.hasInvincible)
        {
            entity.add(new Invincible(message.invincibleDuration));
        }
        
        if (message.hasSpicePower)
        {
            entity.add(new SpicePower(message.spicePower));
        }

        if (message.hasName)
        {
            entity.add(new Name(message.name));
        }

        if (message.HasStats)
        {
            entity.add(new Stats(message.Score, message.Kills));
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
        m_systemGrowthHandler.add(entity);
        m_systemWormMovement.add(entity);
        m_renderer.add(entity);
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
        if (!m_entities.ContainsKey(id))
            return;
        m_entities.Remove(id);
        m_systemKeyboardInput.remove(id);
        m_systemGrowthHandler.remove(id);
        m_systemWormMovement.remove(id);
        m_systemNetwork.remove(id);
        m_renderer.remove(id);
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
    
    
    private void handleNewAnchorPoint(Shared.Messages.NewAnchorPoint message)
    {
        if (m_entities.ContainsKey(message.wormHeadId) && !m_entities.Values.ToArray()[0].id.Equals(message.wormHeadId))
        {
            var wormHead = m_entities[message.wormHeadId];
            var worm = WormMovement.getWormFromHead(wormHead, m_entities);
            foreach (var segment in worm.Skip(1))
            {
                segment.get<AnchorQueue>().m_anchorPositions.Enqueue( new Position(message.position, message.orientation));
            }
        }
    }
    
    private void handleCollision(Shared.Messages.Collision message)
{
    // We need to know if the collision occurred on the screen of the client
    if (m_entities.ContainsKey(message.entity1Id) && m_entities.ContainsKey(message.entity2Id))
    {
        // Check where our current client is and see if the collision is relevant
        var player = m_entities.Values.ToArray()[0];
        // TODO: Implement the check for the client's screen here. If it is in the screen then we will handle the collision
        

        if (message.collisionType == Collision.CollisionType.HeadToSpice)
        {
            // play eat sound
            m_eatSpiceSound.Play();
            Vector2 foodPosition = new Vector2(message.position.X, message.position.Y);
            eatParticleSystem.FoodEaten(foodPosition);

            // TODO: Spice particle effect collision flag
        }

        else if (message.collisionType == Collision.CollisionType.HeadToWall)
        {
            // play death sound
            m_deathSoundInstance.Play();


            Vector2 deathPosition = new Vector2(message.position.X, message.position.Y);

            deathParticleSystem.SnakeDeath(deathPosition);

            // TODO: Wall particle effect collision flag
        }

        else{
            // play death sound
            m_deathSoundInstance.Play();
            Vector2 deathPosition = new Vector2(message.position.X, message.position.Y);

            deathParticleSystem.SnakeDeath(deathPosition);
        }

        

        // We hit another worm

        // Grab the entities
        var entity1 = m_entities[message.entity1Id];
        var entity2 = m_entities[message.entity2Id];
        // Check the position
        var position = message.position;
        

        
        // If it is relevant, we either send a boolean flag to the particle system and collision handling or we call those here. 
        
        // TODO: Implement this
        
    }
}
}

