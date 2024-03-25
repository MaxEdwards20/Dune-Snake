﻿using System;
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
        private Texture2D m_background;
        private SpriteFont m_font;
        private bool isKeyboardRegistered = false;
        private MenuStateEnum newState = MenuStateEnum.GamePlay;
        private GameScores highScores = new GameScores();
        private int countdown = 3;
        private TimeSpan countdownTime = TimeSpan.FromSeconds(1);
        private bool isLoadedScores = false;
        private int playerScore = 0;
        private GameModel m_gameModel;

        public override void initializeSession()
        {
            m_gameModel = new GameModel();
            m_gameModel.initialize(m_contentManager);
        }
        public override void loadContent(ContentManager contentManager)
        {
            m_contentManager = contentManager;
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            // Load background
            m_background = contentManager.Load<Texture2D>("Images/background");
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (!isKeyboardRegistered){RegisterCommands();}
            if (!isLoadedScores)
            {
                highScores.LoadScores();
                isLoadedScores = true;
            }
            keyboardInput.Update(gameTime);
            if (newState != MenuStateEnum.GamePlay){return handleSwitchToMainMenu();}
            return MenuStateEnum.GamePlay;
        }

        public override void update(GameTime gameTime)
        {
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            renderBackground(m_spriteBatch);
            m_spriteBatch.DrawString(m_font, "Score: " + playerScore, new Vector2(10, 10), Color.White);
            m_spriteBatch.End();
            m_gameModel.render(gameTime.ElapsedGameTime, m_spriteBatch);

        }

        private void renderBackground(SpriteBatch m_spriteBatch) {
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
        }

        private void resetGame() {
            countdown = 3;
            countdownTime = TimeSpan.FromSeconds(1);
        }

        public override void RegisterCommands()
        {
            keyboardInput.registerCommand(keyboardInput.Escape, true, Escape);
            keyboardInput.registerCommand(keyboardInput.SnakeDown, false, MoveDown);
            keyboardInput.registerCommand(keyboardInput.SnakeUp, false, MoveUp);
            keyboardInput.registerCommand(keyboardInput.SnakeLeft, false, MoveLeft);
            keyboardInput.registerCommand(keyboardInput.SnakeRight, false, MoveRight);
            isKeyboardRegistered = true;
        }

        public void MoveUp(GameTime gameTime, float scale)
        {
        }
        
        public void MoveDown(GameTime gameTime, float scale)
        {
        }

        public void MoveLeft(GameTime gameTime, float scale)
        {
        }

        public void MoveRight(GameTime gameTime, float scale)
        {
        }
        
        public void Escape(GameTime gameTime, float scale)
        {
            newState = MenuStateEnum.MainMenu;
        }


        private MenuStateEnum handleSwitchToMainMenu()
        {
            keyboardInput.ClearAllCommands();
            isKeyboardRegistered = false;
            highScores.addScore(playerScore); // this adds the score and saves to the hardrive asyncronously
            playerScore = 0; // Reset the score
            isLoadedScores = false;
            resetGame();
            var temp = newState;
            newState = MenuStateEnum.GamePlay;
            return temp;
        }

        private void handleCollisions() {
        }
        
    }
}