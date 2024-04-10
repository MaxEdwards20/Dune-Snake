using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Client.IO;
using System.Runtime.Serialization;
using Client.Components;
using Microsoft.Xna.Framework.Input;
using Shared.Components;

namespace Client.Menu
{
    public class GamePlayView : GameStateView
    {
        private ContentManager m_contentManager;
        private bool m_isSetup;
        private bool m_isKeyboardRegistered;
        private MenuStateEnum m_newState;
        private GameModel m_gameModel;
        private TimeSpan m_connectToServerTime = TimeSpan.Zero;
        private Controls m_controls;
        
        public GamePlayView(Controls controls)
        {
            m_controls = controls;
        }
        
        public override void initialize()
        {
            m_gameModel = new GameModel();
            m_gameModel.initialize(m_contentManager, m_controls, m_graphics);
            m_isSetup = false;
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

            //if (!m_isSetup)
            //{
            //    setup(gameTime);
            //}

            MenuKeyboardInput.Update(gameTime); // essentially just checking for whether we have escaped to the main menu
            if (m_newState != MenuStateEnum.GamePlay){return handleSwitchToMainMenu();}
            return MenuStateEnum.GamePlay;
        }

        //private void setup(GameTime gameTime)
        //{
        //    if (m_connectToServerTime == TimeSpan.Zero)
        //    {
        //        m_connectToServerTime = TimeSpan.FromSeconds(2); // Try to connect every 2 seconds
        //        var res = connectToServer();
        //        if (res)
        //        {
        //            m_isSetup = true;
        //        }
        //    }
        //    else
        //    {
        //        m_connectToServerTime -= gameTime.ElapsedGameTime;
        //        if (m_connectToServerTime <= TimeSpan.Zero)
        //        {
        //            m_connectToServerTime = TimeSpan.Zero;
        //        }
        //    }
        //}

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

        //private bool connectToServer()
        //{
        //    return MessageQueueClient.instance.initialize("localhost", 3000);
        //}

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
                MessageQueueClient.shutdown();
            }
            catch (SocketException e)
            {
                // Console.WriteLine(e); // This happens if we were not able to connect to the server and try to exit from the menu screen
            }
            return temp;
        }
    }
}
