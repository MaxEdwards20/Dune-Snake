using Client.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Menu
{
    public interface IGameState
    {
        void initializeSession();
        void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, MenuKeyboardInput menuKeyboardInput);
        void loadContent(ContentManager contentManager);
        MenuStateEnum processInput(GameTime gameTime);
        void update(GameTime gameTime);
        void render(GameTime gameTime);
    }
}