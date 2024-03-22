using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CS5410.IO;

namespace CS5410.Menu
{
    public class HelpView : GameStateView
    {
        private SpriteFont m_font;
        private const string MESSAGE = "Win by landing on the platform.\nUse the arrow keys (unless otherwise configured) to move.\nThe less fuel you use, the higher your score.\nGood luck!";
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
            keyboardInput.Update(gameTime);
            if (newState != MenuStateEnum.Help) { 
                keyboardInput.ClearAllCommands();
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
            keyboardInput.registerCommand(keyboardInput.Escape, true, new IInputDevice.CommandDelegate(Escape));
            isKeyboardRegistered = true;    
        }

        private void Escape(GameTime gameTime, float scale)
        {
            newState = MenuStateEnum.MainMenu;
        }
    }
}
