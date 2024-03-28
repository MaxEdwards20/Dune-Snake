
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;
using System;
using Shared.Components;
using Shared.Components.Appearance;

namespace Client.Systems
{
    public class Renderer : Shared.Systems.System
    {

        public Renderer() :
            base(
                typeof(Client.Components.Sprite),
                typeof(Shared.Components.Position),
                typeof(Shared.Components.Size)
                )
        {

        }

        public override void update(TimeSpan elapsedTime) { }

        public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            foreach (Entity entity in m_entities.Values)
            {
                if (entity.contains<Head>() || entity.contains<Tail>() || entity.contains<Body>())
                {
                    renderSandWorm(elapsedTime, spriteBatch, entity);
                } else
                {
                    renderEntity(elapsedTime, spriteBatch, entity);
                }
            }
            spriteBatch.End();
        }
        
        private void renderSandWorm(TimeSpan elapsedTime, SpriteBatch spriteBatch, Entity entity)
        { 
            renderSandwormHead(elapsedTime, spriteBatch, entity);
            renderSandwormBody(elapsedTime, spriteBatch, entity);
            renderSandwormTail(elapsedTime, spriteBatch, entity);
        }
        
        private void renderEntity(TimeSpan elapsedTime, SpriteBatch spriteBatch, Entity entity)
        {
            var position = entity.get<Shared.Components.Position>().position;
            var orientation = entity.get<Shared.Components.Position>().orientation;
            var size = entity.get<Shared.Components.Size>().size;
            var texture = entity.get<Components.Sprite>().texture;
            var texCenter = entity.get<Components.Sprite>().center;

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
        }
        
        private void renderSandwormHead(TimeSpan elapsedTime, SpriteBatch spriteBatch, Entity entity)
        {
            var position = entity.get<Shared.Components.Position>().position;
            var orientation = entity.get<Shared.Components.Position>().orientation;
            var size = entity.get<Shared.Components.Size>().size;
            var texture = entity.get<Shared.Components.Appearance.Head>().texture;
            var color = entity.get<Shared.Components.Appearance.Head>().color;
            var texCenter = entity.get<Components.Sprite>().center;
        }

        private void renderSandwormBody(TimeSpan elapsedTime, SpriteBatch spriteBatch, Entity entity)
        {
        }
        

        
        private void renderSandwormTail(TimeSpan elapsedTime, SpriteBatch spriteBatch, Entity entity)
        {
        }
    }
}
