using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Shared.Entities;
using Shared.Components;
using Shared.Components.Appearance;

using System;
using System.Collections.Generic;
using Client.Components;
using Client.Menu;

namespace Client.Systems;

public class Renderer : Shared.Systems.System
{
    private Systems.Camera m_camera;
    private GraphicsDeviceManager m_graphics;
    private SpriteFont m_font;
    private Texture2D m_sand;

    public Renderer(Systems.Camera camera, GraphicsDeviceManager graphics, SpriteFont font, Texture2D sand) :
        base(
           typeof(Position), typeof(Sprite)
        )
    {
        m_camera = camera;
        m_graphics = graphics;
        m_font = font;
        m_sand = sand;
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

        //spriteBatch.Begin();
        spriteBatch.Begin(transformMatrix: matrix);

        Rectangle rect = new(0, 0, 100, 100);
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
        var bodies = new List<Entity>();
        var tails = new List<Entity>();
        var others = new List<Entity>();

        foreach (Entity entity in m_entities.Values)
        {
            if (entity.contains<Head>())
                heads.Add(entity);
            else if (entity.contains<Tail>())
                tails.Add(entity);
            else if (entity.contains<Worm>())
                bodies.Add(entity);
            else
                others.Add(entity);
        }

        foreach (Entity entity in others)
        {
            renderEntity(elapsedTime, spriteBatch, entity);
        }
        foreach (Entity entity in tails)
        {
            renderEntity(elapsedTime, spriteBatch, entity);
        }
        foreach (Entity entity in bodies)
        {
            renderEntity(elapsedTime, spriteBatch, entity);
        }
        foreach (Entity entity in heads)
        {
            renderEntity(elapsedTime, spriteBatch, entity);
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

        // Build a rectangle centered at position, with width/height of size
        Rectangle rectangle = new Rectangle(
            (int)(position.X - size.X / 2),
            (int)(position.Y - size.Y / 2),
            (int)size.X,
            (int)size.Y);

        spriteBatch.Draw(
            texture,
            rectangle,
            null,
            Color.White,
            orientation,
            texCenter,
            SpriteEffects.None,
            0);

        if (entity.contains<Name>())
        {
            // We want the name position to be above the head
            Vector2 namePosition = new Vector2(position.X - size.X + 10, position.Y - size.Y - 10);
            Drawing.DrawPlayerName(m_font, entity.get<Name>().name, namePosition, Color.White, spriteBatch);
        }
    }
}
