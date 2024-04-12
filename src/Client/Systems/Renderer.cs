using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Shared.Entities;
using Shared.Components;

using System;
using System.Collections.Generic;
using Client.Components;
using Client.Menu;
using Shared.Systems;

namespace Client.Systems;

public class Renderer : Shared.Systems.System
{
    private Systems.Camera m_camera;
    private GraphicsDeviceManager m_graphics;
    private SpriteFont m_font;
    private SpriteFont m_fontSmall;
    private Texture2D m_sand;
    private List<Rectangle> m_backgroundTiles = new();

    public Renderer(Systems.Camera camera, GraphicsDeviceManager graphics, SpriteFont font, SpriteFont fontSmall, Texture2D sand) :
        base(
           typeof(Position), typeof(Sprite)
        )
    {
        m_camera = camera;
        m_graphics = graphics;
        m_font = font;
        m_fontSmall = fontSmall;
        m_sand = sand;

        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                m_backgroundTiles.Add(new Rectangle(-100 + i * 500, -100 + j * 500, 500, 500));
    }

    public override void update(TimeSpan elapsedTime) { }

    public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
    {
        float scale = m_camera.Zoom;
        Matrix matrix = Matrix.Identity;
        Vector2 offset = -m_camera.Viewport.Location.ToVector2()
            + new Vector2(m_camera.Viewport.Width, m_camera.Viewport.Height) / scale / 2;
        float scaleX = m_graphics.PreferredBackBufferWidth / (float)m_camera.Viewport.Width * scale;
        float scaleY = m_graphics.PreferredBackBufferHeight / (float)m_camera.Viewport.Height * scale;

        matrix *= Matrix.CreateTranslation(new Vector3(offset, 0));
        matrix *= Matrix.CreateScale(scaleX, scaleY, 1);

        spriteBatch.Begin(transformMatrix: matrix);

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

        var heads = new List<Entity>();
        var others = new List<Entity>();

        foreach (Entity entity in m_entities.Values)
        {
            if (entity.contains<Head>())
                heads.Add(entity);
            else if (entity.contains<Worm>())
                continue;
            else
                others.Add(entity);
        }

        foreach (Entity entity in others)
            renderEntity(elapsedTime, spriteBatch, entity);

        // We want to sort bodies by their position in the worm

        foreach (Entity head in heads)
        {
            var worm = WormMovement.getWormFromHead(head, m_entities);
            for (int i = worm.Count - 1; i >= 0; i--)
                renderEntity(elapsedTime, spriteBatch, worm[i]);
        }
        spriteBatch.End();

        // Need to do a spritebatch without matrix transform for HUD
        spriteBatch.Begin();
        foreach (Entity head in heads)
        {
            if (head.contains<Input>())
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
        }
        spriteBatch.End();
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
            var invincible = entity.get<Invincible>();
            color = invincible.duration < 1000 ? Color.Coral : Colors.displayColor;
        }

        if (entity.contains<SpicePower>() && !entity.contains<Worm>())
        {
            var spicePower = entity.get<SpicePower>();
            if (spicePower.power > 6)
                color = Color.Aqua;
            else if (spicePower.power > 3)
                color = Color.Green;
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
