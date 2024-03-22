using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using CS5410.IO;
using CS5410.Objects;
using System.Runtime.Serialization;

namespace CS5410.Menu
{
    public class GamePlayView : GameStateView
    {
        private ContentManager m_contentManager;
        private bool isOnEasy = true;
        private bool isKeyboardRegistered = false;
        private MenuStateEnum newState = MenuStateEnum.GamePlay;
        private Texture2D m_background;
        private int countdown = 3;
        private TimeSpan countdownTime = TimeSpan.FromSeconds(1);
        private SpriteFont m_font;
        private bool isLoadedScores = false;
        private GameScores highScores;
        private int playerScore = 0;
        private bool playerScoreUpdated = false;
        public override void loadContent(ContentManager contentManager)
        {
            m_contentManager = contentManager;
            highScores = new GameScores();
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            // Load background
            // TODO: m_background = contentManager.Load<Texture2D>("Images/background");
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
            handleCollisions();
        }

        public override void render(GameTime gameTime)
        {
            renderBackground();
            drawText();
        }

        private void renderBackground() {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
            m_spriteBatch.End();
        }

        private void drawText() { 
            var scoreText = "Score: " + playerScore;
        }

        private void resetGame() {
            countdown = 3;
            countdownTime = TimeSpan.FromSeconds(1);
            playerScoreUpdated = false;
            highScores = new GameScores();
        }

        public override void RegisterCommands()
        {
            isKeyboardRegistered = true;
        }

        public void MoveUp(GameTime gameTime, float scale)
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
            // newState = MenuStateEnum.MainMenu; // uncomment this to allow the escape key to return to the main menu. I did not implement a pause screen so I commented this out.
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
