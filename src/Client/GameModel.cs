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
using Client.Systems;
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
    private Controls m_controls;
    private GraphicsDeviceManager m_graphics;
    private SpriteFont m_font;
    private SpriteFont m_fontSmall;
    private Texture2D m_sand;
    private String m_playerName;
    private ScoreSystem m_systemScore;
    private PlayerData m_playerData;
    private SoundEffect m_deathSound;
    private SoundEffect m_eatSpiceSound;
    private SoundEffectInstance m_deathSoundInstance;
    private SoundEffectInstance m_eatSpiceSoundInstance;

    private ParticleSystem eatParticleSystem = new ParticleSystem(new Vector2(0, 0), 4, 1, 0.2f, 0.1f, 500, 150);
    private ParticleSystem deathParticleSystem = new ParticleSystem(new Vector2(0, 0), 10, 5, 0.5f, 0.25f, 1500, 500);


    private bool collisionIsOn;
    private bool spiceEaten;

    private int clientId;


    public GameModel(StringBuilder playerName)
    {
        m_playerName = playerName.ToString();
    }

    /// <summary>
    /// This is where everything performs its update.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
        m_systemNetwork.update(elapsedTime, MessageQueueClient.instance.getMessages(), m_playerData);
        m_systemKeyboardInput.update(elapsedTime);
        m_systemWormMovement.update(elapsedTime);
        m_systemInterpolation.update(elapsedTime);
        m_systemCamera.update(elapsedTime);
        m_renderer.update(elapsedTime);
        GameTime gameTime = new GameTime(totalGameTime: TimeSpan.Zero, elapsedGameTime: elapsedTime);
        eatParticleSystem.update(gameTime);
        deathParticleSystem.update(gameTime);
    }

    /// <summary>
    /// Where we render everything
    /// </summary>

    public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
    {
        m_renderer.render(elapsedTime, spriteBatch, m_playerData, eatParticleSystem, deathParticleSystem);
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
        

        m_contentManager = contentManager;
        m_systemScore = new ScoreSystem();
        m_playerData = new PlayerData(0, m_playerName);
        m_entities = new Dictionary<uint, Entity>();
        m_systemInterpolation = new Systems.Interpolation();
        m_systemCamera = new Systems.Camera(new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), m_playerData);

        m_renderer = new Systems.Renderer(m_systemCamera, graphics, m_font, m_fontSmall, m_sand, m_contentManager);
        m_systemWormMovement = new Shared.Systems.WormMovement();
        m_systemNetwork = new Systems.Network(m_playerName, m_playerData);

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
            createAppearance(message, entity);
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

        if (message.hasClientId)
        {
            entity.add(new Shared.Components.ClientId(message.clientId));
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

    private void createAppearance(NewEntity message, Entity entity)
    {
        var isSpice = message.hasSpicePower && !message.hasWorm;
        if (isSpice)
        {
            createSpiceAppearance(message, entity);
        }
        else if (message.hasHead)
        {
            Texture2D texture = m_contentManager.Load<Texture2D>("Textures/sandworm_head");
            entity.add(new Components.Sprite(texture));
        }
        else if (message.hasTail)
        {
            Texture2D texture = m_contentManager.Load<Texture2D>("Textures/sandworm_tail");
            entity.add(new Components.Sprite(texture));
        }
        else if (message.hasWorm)
        {
            Texture2D texture = m_contentManager.Load<Texture2D>("Textures/sandworm_body");
            entity.add(new Components.Sprite(texture));
        }
        else if (message.hasWall)
        {
            Texture2D texture = m_contentManager.Load<Texture2D>("Textures/wall");
            entity.add(new Components.Sprite(texture));
        }
        else
        {
            Texture2D texture = m_contentManager.Load<Texture2D>(message.texture);
            entity.add(new Components.Sprite(texture));
        }
    }

    private void createSpiceAppearance(NewEntity message, Entity entity)
    {
        int row = 0;
        if (message.spicePower > 7)
        {
            row = 2;
        }
        else if (message.spicePower > 5)
        {
            row = 3;
        }
        else if (message.spicePower > 3)
        {
            row = 1;
        }
        else if (message.spicePower > 1)
        {
            row = 6;
        }

        Texture2D texture = m_contentManager.Load<Texture2D>("Textures/rotating_orbs");
        entity.add(new Components.Sprite(texture, true, row));
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
        m_systemScore.SaveScore(m_entities[id]); // We call this every time
        m_entities.Remove(id);
        m_systemKeyboardInput.remove(id);
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
                segment.get<AnchorQueue>().m_anchorPositions.Enqueue(new Position(message.position, message.orientation));
            }
        }
    }

    
    private Entity getPlayer()
    {
        foreach (var e in m_entities.Values)
        {
            if (e.contains<Name>() && e.get<Name>().name == m_playerName)
            {
                return e;
            }
        }
        return null;
    }

    private void handleCollision(Shared.Messages.Collision message)
    {
        // We need to know if the collision occurred on the screen of the client
        if (m_entities.ContainsKey(message.senderId) && m_entities.ContainsKey(message.receiverId))
        {
            // Grab the entities
            var entity1 = m_entities[message.senderId];
            var entity2 = m_entities[message.receiverId];

            // Check for sound on the player
            handleCollisionSounds(message, entity1, entity2);
            
            if (message.collisionType == Collision.CollisionType.HeadToSpice)
            {
                Vector2 foodPosition = new Vector2(message.position.X, message.position.Y);
                eatParticleSystem.FoodEaten(foodPosition);
            } else if (message.collisionType == Collision.CollisionType.HeadToWall || message.collisionType == Collision.CollisionType.SenderDies)
            {
                Vector2 deathPosition = new Vector2(message.position.X, message.position.Y);
                var worm = WormMovement.getWormFromHead(entity1, m_entities);
                deathParticleSystem.SnakeDeath(deathPosition, worm);
            } else if (message.collisionType == Collision.CollisionType.ReceiverDies)
            {
                Vector2 deathPosition = new Vector2(message.position.X, message.position.Y);
                var worm = WormMovement.getWormFromHead(entity2, m_entities);
                deathParticleSystem.SnakeDeath(deathPosition, worm);
                deathParticleSystem.SnakeKilled(deathPosition);
            } 
        }
    }

    private void handleCollisionSounds(Collision message,  Entity entity1, Entity entity2)
    {
        // Check where our current client is and see if the collision is relevant
        var player = getPlayer();
        if (player == null)
        {
            return;
        }
        if (message.collisionType == Collision.CollisionType.ReceiverDies && player == entity1 ||
            message.collisionType == Collision.CollisionType.SenderDies && player == entity2)
        {
            m_deathSoundInstance.Play();
        }

        if (message.collisionType == Collision.CollisionType.HeadToSpice && player == entity1)
        {
            m_eatSpiceSoundInstance.Play();
        }
    }
}

