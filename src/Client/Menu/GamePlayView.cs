using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Client.IO;
using Client.Objects;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;

namespace Client.Menu
{
    public class GamePlayView : GameStateView
    {
        private ContentManager m_contentManager;
        private bool m_isSetup;
        private MenuStateEnum m_newState;
        private GameModel m_gameModel;
        private TimeSpan m_connectToServerTime = TimeSpan.Zero;
        
        public override void initialize()
        {
            m_gameModel = new GameModel();
            m_gameModel.initialize(m_contentManager);
            m_isSetup = false;
            m_newState = MenuStateEnum.GamePlay;
        }
        public override void loadContent(ContentManager contentManager)
        {
            m_contentManager = contentManager;
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (!m_isSetup)
            {
                setup(gameTime);
            }
            MenuKeyboardInput.Update(gameTime); // essentially just checking for whether we have escaped to the main menu
            if (m_newState != MenuStateEnum.GamePlay){return handleSwitchToMainMenu();}
            return MenuStateEnum.GamePlay;
        }

        private void setup(GameTime gameTime)
        {
            RegisterCommands();
            var res = connectToServer();
            if (res)
            {
                m_isSetup = true; // We only want to say we are setup when we are connected to the server
            }
            else
            {
                if (m_connectToServerTime == TimeSpan.Zero)
                {
                    res = connectToServer();
                    m_connectToServerTime = TimeSpan.FromSeconds(2);
                    m_connectToServerTime -= gameTime.ElapsedGameTime;
                }
            }
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

        private bool connectToServer()
        {
            return MessageQueueClient.instance.initialize("localhost", 3000);
        }

        private void escape(GameTime gameTime, float scale)
        {
            m_newState = MenuStateEnum.MainMenu;
        }
        
        private MenuStateEnum handleSwitchToMainMenu()
        {
            MenuKeyboardInput.ClearAllCommands();
            m_isSetup = false;
            var temp = m_newState;
            m_newState = MenuStateEnum.GamePlay;
            try
            {
                MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
                MessageQueueClient.instance.shutdown();
            }
            catch (SocketException e)
            {
                // Console.WriteLine(e); // This happens if we were not able to connect to the server and try to exit from it
            }
            return temp;
        }
    }
}
