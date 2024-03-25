using Client.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Menu
{
    public class HelpView : GameStateView
    {
        private SpriteFont m_font;
        private const string MESSAGE = "Eat food to grow.\nAvoid other sandworms until you are large enough to consume them.\nUse the arrow keys to move and press escape to return to the main menu.\nGood Luck!";
        private bool isKeyboardRegistered = false;
        private MenuStateEnum newState = MenuStateEnum.Help;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
        }

         public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (!isKeyboardRegistered)
            {
                RegisterCommands();
            }
            MenuKeyboardInput.Update(gameTime);
            if (newState != MenuStateEnum.Help) { 
                MenuKeyboardInput.ClearAllCommands();
                isKeyboardRegistered = false;
                var transState = newState;
                newState = MenuStateEnum.Help;
                return transState;
            } 
            return MenuStateEnum.Help;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            Drawing.DrawShadedString(m_font, MESSAGE, new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2), Colors.displayColor ,m_spriteBatch, boxed: true);
            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
        }

        public override void RegisterCommands()
        {
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Escape, true, new IInputDevice.CommandDelegate(Escape));
            isKeyboardRegistered = true;    
        }

        private void Escape(GameTime gameTime, float scale)
        {
            newState = MenuStateEnum.MainMenu;
        }
    }
}
