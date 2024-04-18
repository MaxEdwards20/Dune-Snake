using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Shared.Entities;
using Shared.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using Client.Components;
using Client.Menu;
using CS5410;
using Microsoft.Xna.Framework.Content;
using Shared.Systems;
using Wall = Shared.Entities.Wall;

namespace Client.Systems;

public class Renderer : Shared.Systems.System
{
    private Systems.Camera m_camera;
    private GraphicsDeviceManager m_graphics;
    private SpriteFont m_font;
    private SpriteFont m_fontSmall;
    private Texture2D m_sand;
    private List<Rectangle> m_backgroundTiles = new();
    private ParticleSystemRenderer deathRenderer;
    private ParticleSystemRenderer eatRenderer;

    public Renderer(Systems.Camera camera, GraphicsDeviceManager graphics, SpriteFont font, SpriteFont fontSmall, Texture2D sand, ContentManager contentManager) :
        base(
           typeof(Position), typeof(Sprite)
        )
    {
        m_camera = camera;
        m_graphics = graphics;
        m_font = font;
        m_fontSmall = fontSmall;
        m_sand = sand;
        deathRenderer = new ParticleSystemRenderer("Textures/particle");
        eatRenderer = new ParticleSystemRenderer("Textures/particle");
        
        
        eatRenderer.LoadContent(contentManager);
        deathRenderer.LoadContent(contentManager);

        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                m_backgroundTiles.Add(new Rectangle(-100 + i * 500, -100 + j * 500, 500, 500));
    }

    public override void update(TimeSpan elapsedTime)
    {
        foreach (var entity in m_entities.Values)
        {
            var sprite = entity.get<Sprite>();
            if (!sprite.isAnimated) continue;
            var animation = sprite;
            animation.Timer += (float)elapsedTime.TotalMilliseconds;
            
            if (animation.Timer > animation.FrameSpeed)
            {
                animation.Timer = 0f;
                animation.CurrentFrame++;

                if (animation.CurrentFrame >= animation.FrameCount)
                {
                    animation.CurrentFrame = animation.IsLooping ? 0 : animation.FrameCount - 1;
                }

                // Update the sprite's source rectangle for the new frame
                sprite.SourceRect = new Rectangle(
                    sprite.SourceRect.Width * animation.CurrentFrame, // X position of frame
                    sprite.SourceRect.Y,                              // Y is constant
                    sprite.SourceRect.Width,                          // Frame width
                    sprite.SourceRect.Height                          // Frame height
                );
            }
        }
    }
    

    public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch, PlayerData playerData, ParticleSystem eatParticles, ParticleSystem deathParticles)
    {
        // Setup variables
        float scale = m_camera.Zoom;
        Matrix matrix = Matrix.Identity;
        Vector2 offset = -m_camera.Viewport.Location.ToVector2()
            + new Vector2(m_camera.Viewport.Width, m_camera.Viewport.Height) / scale / 2;
        float scaleX = m_graphics.PreferredBackBufferWidth / (float)m_camera.Viewport.Width * scale;
        float scaleY = m_graphics.PreferredBackBufferHeight / (float)m_camera.Viewport.Height * scale;

        matrix *= Matrix.CreateTranslation(new Vector3(offset, 0));
        matrix *= Matrix.CreateScale(scaleX, scaleY, 1);

        var heads = new List<Entity>();
        var walls = new List<Entity>();
        var spice = new List<Entity>();
        var others = new List<Entity>();
        sortEntities(heads, spice, walls, others);
        
        // Begin drawing
        spriteBatch.Begin(transformMatrix: matrix);
        drawBackgroundTiles(spriteBatch);
        foreach (Entity entity in others) renderEntity(elapsedTime, spriteBatch, entity);
        foreach (Entity entity in spice) renderSpice(elapsedTime, spriteBatch, entity);
        foreach (Entity entity in walls) renderEntity(elapsedTime, spriteBatch, entity);
        drawWorms(elapsedTime, spriteBatch, heads);
        eatRenderer.draw(spriteBatch, eatParticles );
        deathRenderer.draw(spriteBatch, deathParticles);
        spriteBatch.End();
        
        // Need to do a spritebatch without matrix transform for HUD
        spriteBatch.Begin();
        var isGameOver = true;
        foreach (Entity head in heads)
        {
            if (head.contains<Input>())
            {
                isGameOver = false;
                drawStats(spriteBatch, head);
            }
        }
        drawGameOverScreen(spriteBatch, playerData, isGameOver);
        drawLeaderboard(spriteBatch, heads);
        spriteBatch.End();
    }
    
    private void drawGameOverScreen(SpriteBatch spriteBatch, PlayerData playerData, bool isGameOver)
    {
        if (!isGameOver) return;
        Drawing.DrawBlurredRectangle(spriteBatch, new Vector2(m_graphics.PreferredBackBufferWidth / 2 - 200, m_graphics.PreferredBackBufferHeight / 2 - 150), new Vector2(400, 300), 7, transparency:0.6f);
        Drawing.CustomDrawString(m_font, "Game Over", new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2 - 100), Color.White, spriteBatch, centered: true);
        Drawing.CustomDrawString(m_font, "Final Score: " + playerData.score, new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2 - 50), Color.White, spriteBatch, centered: true);
        Drawing.CustomDrawString(m_font, "Highest Rank: " + playerData.highestPosition, new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2 ), Color.White, spriteBatch, centered: true);
        Drawing.CustomDrawString(m_font, "Kills: " + playerData.kills, new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2 + 50), Color.White, spriteBatch, centered: true);
        Drawing.CustomDrawString(m_fontSmall, "Press Escape to Return to Menu", new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2 + 100), Color.White, spriteBatch, centered: true, boxed: true);
    }

    private void drawWorms(TimeSpan elapsedTime, SpriteBatch spriteBatch, List<Entity> heads)
    {
        foreach (Entity head in heads)
        {
            var worm = WormMovement.getWormFromHead(head, m_entities);
            for (int i = worm.Count - 1; i >= 0; i--)
                renderEntity(elapsedTime, spriteBatch, worm[i]);
        }
    }

    private void sortEntities(List<Entity> heads, List<Entity> animatedSprites, List<Entity> walls, List<Entity> others)
    {
        foreach (Entity entity in m_entities.Values)
        {
            if (entity.contains<Head>())
                heads.Add(entity);
            else if (entity.contains<Worm>())
                continue;
            else if (entity.contains<Sprite>() && entity.get<Sprite>().isAnimated)
                animatedSprites.Add(entity);
            else if (entity.contains<Shared.Components.Wall>())
                walls.Add(entity);
            else
                others.Add(entity);
        }
    }

    private void drawBackgroundTiles(SpriteBatch spriteBatch)
    {
        foreach (Rectangle rect in m_backgroundTiles)
            spriteBatch.Draw(
                m_sand,
                rect,
                null,
                Color.White,
                0,
                Vector2.Zero,
                SpriteEffects.None,
                0
            );
    }

    private void drawLeaderboard(SpriteBatch spriteBatch, List<Entity> heads)
    {
        int numToDisplay = 5;
        var scoresToDisplay = new List<(uint, string)>();
        // First we need to create the scores to display by goin through each head and grabbing its score
        foreach (Entity head in heads.Take(numToDisplay))
        {
            Stats stats = head.get<Stats>();
            scoresToDisplay.Add((stats.Score, head.get<Name>().name));
        }
        // Then we sort the scores by the first element of each tuple, the score
        scoresToDisplay.Sort((a, b) => b.Item1.CompareTo(a.Item1));
        
        
        Drawing.DrawBlurredRectangle(spriteBatch, new Vector2(m_graphics.PreferredBackBufferWidth - 350, 0), new Vector2(350, scoresToDisplay.Count * 50 + 100), 7);
        Drawing.CustomDrawString(m_font, "Leaderboard", new Vector2(m_graphics.PreferredBackBufferWidth - 300, 0), Color.White, spriteBatch, centered: false);
        // Then we draw the scores
        for (int i = 0; i < scoresToDisplay.Count; i++)
        {
            Drawing.CustomDrawString(
                m_font,
                scoresToDisplay[i].Item2 + ": " + scoresToDisplay[i].Item1.ToString(),
                new Vector2(m_graphics.PreferredBackBufferWidth - 300, i * 50 + 75),
                Color.White,
                spriteBatch,
                centered: false, 
                scale:.7f
            );
        }
        
    }

    private void drawStats(SpriteBatch spriteBatch, Entity head)
    {
        Stats stats = head.get<Stats>();
        Drawing.CustomDrawString(
            m_font,
            "Score: " + stats.Score.ToString(),
            new Vector2(0, 0),
            Color.White,
            spriteBatch,
            centered: false,
            boxed: true
        );

        Drawing.CustomDrawString(
            m_font,
            "Kills: " + stats.Kills.ToString(),
            new Vector2(0, m_graphics.PreferredBackBufferHeight * 0.10f),
            Color.White,
            spriteBatch,
            centered: false,
            boxed: true
        );
    }
    
    private void renderSpice(TimeSpan elapsedTime, SpriteBatch spriteBatch, Entity entity)
    {
        var sprite = entity.get<Sprite>();
        var position = entity.get<Position>().position;
        var rotation = entity.contains<Position>() ? entity.get<Position>().orientation : 0f;
        var scale = entity.get<SpicePower>().power / 3;

        spriteBatch.Draw(
            sprite.texture,
            position,
            sprite.SourceRect,          // Use the source rectangle which is set by the AnimationSystem
            Color.White,                // Assuming white means "use texture colors"
            rotation,
            new Vector2(sprite.SourceRect.Width / 2, sprite.SourceRect.Height / 2),  // Origin of the rotation
            scale,
            SpriteEffects.None,
            0f
        );
    }

    private void renderEntity(TimeSpan elapsedTime, SpriteBatch spriteBatch, Entity entity)
    {
        var position = entity.get<Shared.Components.Position>().position;
        var orientation = entity.get<Shared.Components.Position>().orientation;
        var size = entity.get<Shared.Components.Size>().size;
        var texCenter = entity.get<Components.Sprite>().center;
        var texture = entity.get<Components.Sprite>().texture;
        var color = Color.White;

        if (entity.contains<Invincible>())
        {
            color = Color.Coral;
        }
        
        // Build a rectangle centered at position, with width/height of size
        Rectangle rectangle = new(
            (int)position.X,
            (int)position.Y,
            (int)size.X,
            (int)size.Y);

        spriteBatch.Draw(
            texture,
            rectangle,
            null,
            color,
            orientation,
            texCenter,
            SpriteEffects.None,
            0);

        if (entity.contains<Name>())
        {
            // We want the name position to be above the head
            Drawing.CustomDrawString(
                m_fontSmall,
                entity.get<Name>().name,
                new(position.X, position.Y - size.Y / 2),
                Color.White,
                spriteBatch,
                boxed: true
                );
        }
    }
}