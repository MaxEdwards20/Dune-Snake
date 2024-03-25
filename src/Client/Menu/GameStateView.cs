using System.ComponentModel;
using Client.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Menu
{
    public abstract class GameStateView : IGameState
    {
        protected GraphicsDeviceManager m_graphics;
        protected SpriteBatch m_spriteBatch;
        protected KeyboardInput keyboardInput;
        public void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, KeyboardInput keyboardInput)
        {
            m_graphics = graphics;
            m_spriteBatch = new SpriteBatch(graphicsDevice);
            this.keyboardInput = keyboardInput;
        }

        // Taken from the ECS SnakeGame Demo Code
        public virtual void initializeSession()
        {
        }

        public abstract void loadContent(ContentManager contentManager);
        public abstract MenuStateEnum processInput(GameTime gameTime);
        public abstract void render(GameTime gameTime);
        public abstract void update(GameTime gameTime);
        public abstract void RegisterCommands();
    }
}
