
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Client.Menu { 

    public class Colors { 
        public static Color selectedColor = Color.Goldenrod;
        public static Color displayColor = Color.PaleGoldenrod;
    }

    public class Drawing { 
        public static void DrawShadedString(SpriteFont font, string message, Vector2 position, Color color, SpriteBatch spriteBatch, bool centered = true, bool boxed = false, bool shaded = true, float scale = 1.0f) {
            if (centered) { 
                position = new Vector2(position.X - font.MeasureString(message).X / 2, position.Y - font.MeasureString(message).Y / 2);  
            }
            if (boxed) { 
                DrawBlurredRectangle(spriteBatch, position, font.MeasureString(message), 5, 0.6f);
            }
            if (shaded) { 
                for (int i = 1; i < 3; i++) {
                    spriteBatch.DrawString(font, message, new Vector2(position.X - i, position.Y - i), Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                }        
            }
            spriteBatch.DrawString(font, message, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        public static void DrawBlurredRectangle(SpriteBatch spriteBatch, Vector2 position, Vector2 size, int blurRadius, float transparency = 0.8f)
        {
            Rectangle blurredRect = new Rectangle((int)(position.X - blurRadius), (int)(position.Y - blurRadius), (int)size.X + blurRadius * 2, (int)size.Y + blurRadius * 2);
            Color color = Color.Black * transparency; // Adjust the color and transparency as needed
            DrawRectangle(spriteBatch, blurredRect, color);
        }

        private static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            Texture2D dummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { color });
            spriteBatch.Draw(dummyTexture, rectangle, color);
        }
        
        public static void DrawPlayerName(SpriteFont font, string name, Vector2 position, Color color, SpriteBatch spriteBatch) {
            // We need to downsize the font for the player name
            DrawShadedString(font, name, position, color, spriteBatch, false, false, true, scale:0.4f);
        }

    }
}