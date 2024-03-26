using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Client.IO;
using Client.Objects;
using System.Runtime.Serialization;

namespace Client.Menu
{
    public class GamePlayView : GameStateView
    {
        private ContentManager m_contentManager;
        private bool m_isKeyboardRegistered;
        private MenuStateEnum m_newState;
        private GameModel m_gameModel;
        
        public override void initializeSession()
        {
            m_gameModel = new GameModel();
            m_gameModel.initialize(m_contentManager);
            m_isKeyboardRegistered = false;
            m_newState = MenuStateEnum.GamePlay;
        }
        public override void loadContent(ContentManager contentManager)
        {
            m_contentManager = contentManager;
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (!m_isKeyboardRegistered){RegisterCommands();}
            MenuKeyboardInput.Update(gameTime); // essentially just checking for whether we have escaped to the main menu
            if (m_newState != MenuStateEnum.GamePlay){return handleSwitchToMainMenu();}
            return MenuStateEnum.GamePlay;
        }

        public override void update(GameTime gameTime)
        {
            m_gameModel.update(gameTime.ElapsedGameTime);
        }

        public override void render(GameTime gameTime)
        {
            m_gameModel.render(gameTime.ElapsedGameTime, m_spriteBatch);
        }
        
        public override void RegisterCommands()
        {
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Escape, true, escape);
            m_isKeyboardRegistered = true;
        }

        private void escape(GameTime gameTime, float scale)
        {
            m_newState = MenuStateEnum.MainMenu;
        }
        
        private MenuStateEnum handleSwitchToMainMenu()
        {
            MenuKeyboardInput.ClearAllCommands();
            m_isKeyboardRegistered = false;
            var temp = m_newState;
            m_newState = MenuStateEnum.GamePlay;
            return temp;
        }
    }
}
