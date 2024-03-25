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
        private Texture2D m_background;
        private SpriteFont m_font;
        private bool m_isKeyboardRegistered = false;
        private MenuStateEnum m_newState = MenuStateEnum.GamePlay;
        private GameScores m_highScores = new GameScores();
        private int m_countdown = 3;
        private TimeSpan m_countdownTime = TimeSpan.FromSeconds(1);
        private bool m_isLoadedScores = false;
        private int m_playerScore = 0;
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
            if (!m_isKeyboardRegistered){RegisterCommands();}
            if (!m_isLoadedScores)
            {
                m_highScores.LoadScores();
                m_isLoadedScores = true;
            }
            MenuKeyboardInput.Update(gameTime);
            if (m_newState != MenuStateEnum.GamePlay){return handleSwitchToMainMenu();}
            return MenuStateEnum.GamePlay;
        }

        public override void update(GameTime gameTime)
        {
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            renderBackground(m_spriteBatch);
            m_spriteBatch.DrawString(m_font, "Score: " + m_playerScore, new Vector2(10, 10), Color.White);
            m_spriteBatch.End();
            m_gameModel.render(gameTime.ElapsedGameTime, m_spriteBatch);

        }

        private void renderBackground(SpriteBatch spriteBatch) {
            spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
        }

        private void resetGame() {
            m_countdown = 3;
            m_countdownTime = TimeSpan.FromSeconds(1);
        }

        public override void RegisterCommands()
        {
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Escape, true, escape);
            m_isKeyboardRegistered = true;
        }

        public void escape(GameTime gameTime, float scale)
        {
            m_newState = MenuStateEnum.MainMenu;
        }


        private MenuStateEnum handleSwitchToMainMenu()
        {
            MenuKeyboardInput.ClearAllCommands();
            m_isKeyboardRegistered = false;
            m_highScores.addScore(m_playerScore); // this adds the score and saves to the hardrive asyncronously
            m_playerScore = 0; // Reset the score
            m_isLoadedScores = false;
            resetGame();
            var temp = m_newState;
            m_newState = MenuStateEnum.GamePlay;
            return temp;
        }

        private void handleCollisions() {
        }
        
    }
}
