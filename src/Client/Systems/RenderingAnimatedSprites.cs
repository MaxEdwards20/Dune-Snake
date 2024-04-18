
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CS5410
{
    public class RenderingAnimatedSprites : Game
    {
        private GraphicsDeviceManager m_graphics;
        private SpriteBatch m_spriteBatch;
/*        private Objects.Bird m_littleBird;
        private Objects.Bird m_bigBird;*/
        private AnimatedSprite m_bigBirdRenderer;
        private AnimatedSprite m_littleBirdRenderer;

        public RenderingAnimatedSprites()
        {
            m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
           

            base.Initialize();
        }

        protected override void LoadContent()
        {
            m_spriteBatch = new SpriteBatch(GraphicsDevice);

           /* m_littleBird = new Objects.Bird(
                new Vector2(75, 75),
                new Vector2(150, 200),
               125 / 1000.0, // Pixels per second
                (float)(Math.PI / 1000.0));

            m_bigBird = new Objects.Bird(
                new Vector2(100, 100),
                new Vector2(150, 350),
                75 / 1000.0, // Pixels per second
                (float)(Math.PI / 1000.0));

            m_bigBirdRenderer = new AnimatedSprite(
                this.Content.Load<Texture2D>("Images/spritesheet-bird"),
                new int[] { 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40 }
            );

            m_littleBirdRenderer = new AnimatedSprite(
                this.Content.Load<Texture2D>("Images/spritesheet-bird"),
                new int[] { 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25 }
            );

           */
        }

        protected override void Update(GameTime gameTime)
        {
           

            m_littleBirdRenderer.update(gameTime);
            m_bigBirdRenderer.update(gameTime);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            m_spriteBatch.Begin();

           /* m_littleBirdRenderer.draw(m_spriteBatch, m_littleBird);
            m_bigBirdRenderer.draw(m_spriteBatch, m_bigBird);*/

            m_spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
