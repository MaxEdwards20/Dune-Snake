
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;
using System;
using Microsoft.Xna.Framework.Content;
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

        public override void update(TimeSpan elapsedTime)
        {
        }

        public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            foreach (Entity entity in m_entities.Values)
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
        }
        
    }
}
