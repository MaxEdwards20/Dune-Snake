using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Client.IO;
using System.Runtime.Serialization;
using System.Text;
using Client.Components;
using Microsoft.Xna.Framework.Input;
using Shared.Components;

namespace Client.Menu
{
    public class GamePlayView : GameStateView
    {
        private ContentManager m_contentManager;
        private bool m_isKeyboardRegistered;
        private MenuStateEnum m_newState;
        private GameModel m_gameModel;
        private Controls m_controls;
        private StringBuilder playerName;
        
        public GamePlayView(Controls controls, StringBuilder name)
        {
            m_controls = controls;
            playerName = name;
        }
        
        public override void initialize()
        {
            m_gameModel = new GameModel(playerName);
            m_gameModel.initialize(m_contentManager, m_controls, m_graphics);
            m_isKeyboardRegistered = false;
            m_newState = MenuStateEnum.GamePlay;
        }
        public override void loadContent(ContentManager contentManager)
        {
            m_contentManager = contentManager;
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (!m_isKeyboardRegistered)
            {
                RegisterCommands();
                m_isKeyboardRegistered = true;
            }
            
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
        }
        
        private void escape(GameTime gameTime, float scale)
        {
            m_newState = MenuStateEnum.MainMenu;
        }
        
        private MenuStateEnum handleSwitchToMainMenu()
        {
            MenuKeyboardInput.ClearAllCommands();
            var temp = m_newState;
            m_newState = MenuStateEnum.GamePlay;
            try
            {
                MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
                MessageQueueClient.shutdown();
                ContentManager m_contentManager = null;
                m_isKeyboardRegistered = false; 
                // m_gameModel = null;
                playerName.Clear();
            }
            catch (SocketException e)
            {
                // Console.WriteLine(e); // This happens if we were not able to connect to the server and try to exit from the menu screen
            }
            return temp;
        }
    }
}
