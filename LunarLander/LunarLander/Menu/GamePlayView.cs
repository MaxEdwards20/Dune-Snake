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
        private Terrain terrain;
        private LunarLander lander;
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
            m_background = contentManager.Load<Texture2D>("Images/StarBackground");

            // Create lander based on size of screen
            var landerSize = new Vector2(60, 60);
            if (m_graphics.PreferredBackBufferWidth < 1500)
            {
                landerSize = new Vector2(50, 50);
            }
            lander = new LunarLander(size: landerSize, graphics: m_graphics);
            lander.loadContent(contentManager);
            var landerRadius = (int)lander.size.X / 2;

            // Create terrain
            terrain = isOnEasy? new Terrain( 2, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight,landerRadius ) : new Terrain(1, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight, landerRadius);
            terrain.loadContent(m_contentManager, m_graphics);
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
            lander.update(gameTime);
            handleCollisions();
            handleLevelChange(gameTime); // this will just return null if the game isn't over
        }

        public override void render(GameTime gameTime)
        {
            renderBackground();
            terrain.render(gameTime, m_spriteBatch);
            lander.render(gameTime, m_spriteBatch);
            drawText();
        }

        private void renderBackground() {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
            m_spriteBatch.End();
        }

        private void drawText() { 
            var scoreText = "Score: " + playerScore;
            var text = "";
            if (lander.isLandedSafely)
            {
                text += "Nice Landing!\nNext Level in: " + countdown;
            }
             else if (lander.isCrashed)
            {
                text += scoreText + "\nYou Crashed!\nReturning to Menu in " + countdown;
            }
            if (text != "")
            {
                m_spriteBatch.Begin();
                Drawing.DrawShadedString(m_font, text,
                    new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2), Colors.displayColor, m_spriteBatch, boxed: true);
                m_spriteBatch.End();
            }
        }

        private void resetGame() {
            lander.reset();
            var zones = isOnEasy ? 2 : 1;
            terrain.reset(zones);
            countdown = 3;
            countdownTime = TimeSpan.FromSeconds(1);
            playerScoreUpdated = false;
            highScores = new GameScores();
        }

        public override void RegisterCommands()
        {
            keyboardInput.registerCommand(keyboardInput.Escape, true, new IInputDevice.CommandDelegate(Escape));
            keyboardInput.registerCommand(keyboardInput.Thrust, false, new IInputDevice.CommandDelegate(MoveUp));
            keyboardInput.registerCommand(keyboardInput.Left, false, new IInputDevice.CommandDelegate(MoveLeft));
            keyboardInput.registerCommand(keyboardInput.Right, false, new IInputDevice.CommandDelegate(MoveRight));
            isKeyboardRegistered = true;
        }

        public void MoveUp(GameTime gameTime, float scale)
        {
            lander.MoveUp(gameTime, scale);
        }

        public void MoveLeft(GameTime gameTime, float scale)
        {
            lander.MoveLeft(gameTime, scale);
        }

        public void MoveRight(GameTime gameTime, float scale)
        {
            lander.MoveRight(gameTime, scale);
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
            isOnEasy = true; // Reset the difficulty
            playerScore = 0; // Reset the score
            isLoadedScores = false;
            resetGame();
            var temp = newState;
            newState = MenuStateEnum.GamePlay;
            return temp;
        }

        private void handleCollisions() { 
            if (CollisionDetector.CheckCollision(lander.m_position, lander.size.X / 2, terrain.terrainPoints)) { 
                var isOnSafeZone = CollisionDetector.IsCollisionOnSafeZone(lander.m_position, lander.size.X / 2, terrain.safeZonePoints);
                lander.handleCollision(isOnSafeZone);
            }
        }

        private void handleLevelChange(GameTime gameTime) {
            if (!(lander.isCrashed || lander.isLandedSafely))
            { // We only want to handle the end of the game if the lander has crashed or landed safely
                return;
            }
            if (!playerScoreUpdated) {
                updatePlayerScore();
                playerScoreUpdated = true;
            }
            countdownTime -= gameTime.ElapsedGameTime;
            if (countdownTime.TotalSeconds <= 0)
            {
                countdown--;
                countdownTime = TimeSpan.FromSeconds(1);
            }
            if (countdown <= 0)
            {
                if (lander.isLandedSafely && isOnEasy)
                {
                    isOnEasy = false; // Up the level
                    resetGame();
                } else if (lander.isLandedSafely && !isOnEasy)
                {
                    playerScore += (int)(lander.m_currentFuel * 150); // more points on hard mode
                    resetGame(); // take us to the next level. We will continue to the next level until the player crashes
                }
                else if (lander.isCrashed)
                {
                    newState = MenuStateEnum.MainMenu;
                }
            }    
        }

        private void updatePlayerScore() {
            if (lander.isLandedSafely && isOnEasy)
            {
                playerScore += (int)(lander.m_currentFuel * 100);
            } else if (lander.isLandedSafely && !isOnEasy)
            {
                playerScore += (int)(lander.m_currentFuel * 150);
            }
        }
    }
}
