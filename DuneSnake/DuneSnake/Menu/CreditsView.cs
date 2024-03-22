using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CS5410.IO;

namespace CS5410.Menu
{
    public class AboutView : GameStateView
    {
        private SpriteFont m_font;
        private const string MESSAGE = "Created by Caden, Satchel, and Max in 2024. Enjoy!";
        private bool isKeyboardRegistered = false;
        private MenuStateEnum newState = MenuStateEnum.Credits;
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

            if (newState != MenuStateEnum.Credits) { 
                keyboardInput.ClearAllCommands();
                isKeyboardRegistered = false;
                var transState = newState;
                newState = MenuStateEnum.Credits;
                return transState;
            } 
            return MenuStateEnum.Credits;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            Drawing.DrawShadedString(m_font, MESSAGE, new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2), Colors.displayColor ,m_spriteBatch);
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
