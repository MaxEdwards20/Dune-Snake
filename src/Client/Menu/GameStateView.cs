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
        protected MenuKeyboardInput MenuKeyboardInput;
        public void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, MenuKeyboardInput menuKeyboardInput)
        {
            m_graphics = graphics;
            m_spriteBatch = new SpriteBatch(graphicsDevice);
            this.MenuKeyboardInput = menuKeyboardInput;
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
