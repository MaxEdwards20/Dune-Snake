// Takes in a terrain object and renders it to the screen

using System;
using System.Collections.Generic;
using CS5410.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410.Objects
{
    public class TerrainRenderer
    {
        private Texture2D m_pixel;
        private int m_safeZones;
        private BasicEffect m_basicEffect;
        private GraphicsDeviceManager m_graphics;
        private int[] m_indexTriStrip;
        private VertexPositionColor[] m_vertsTriStrip;
        private RectangleE[] rectangles;

        public TerrainRenderer(int safeZones)
        {
            m_safeZones = safeZones;
        }

        public void loadContent(ContentManager contentManager, GraphicsDeviceManager m_graphics, List<TPoint> terrainPoints)
        {
            this.m_graphics = m_graphics;
            m_basicEffect = new BasicEffect(m_graphics.GraphicsDevice);
            m_pixel = new Texture2D(m_graphics.GraphicsDevice, 1, 1);
            m_pixel.SetData(new[] { Color.White });
            m_basicEffect.VertexColorEnabled = true;
            newGame(terrainPoints);
        }

        public void newGame(List<TPoint> terrainPoints)
        {
            createTriangleStrip(terrainPoints, Colors.displayColor);
            createAllRectangles(terrainPoints, Colors.selectedColor);
        }

        public void render(SpriteBatch spriteBatch )
        {
            m_basicEffect.VertexColorEnabled = true;
            spriteBatch.Begin();
            DrawTriangleStrip();
            DrawOutline(spriteBatch);
            spriteBatch.End();
        }

        private void createTriangleStrip(List<TPoint> terrainPoints, Color color) { 
            // initialize the arrays
            m_vertsTriStrip = new VertexPositionColor[terrainPoints.Count * 2];
            m_indexTriStrip = new int[terrainPoints.Count * 2];

            for (int i = 0; i < terrainPoints.Count - 1; i++)
            {
                var point1 = terrainPoints[i];
                var point2 = terrainPoints[i + 1];
                var point3 = new TPoint(point2.x, m_graphics.GraphicsDevice.Viewport.Height);
                var point4 = new TPoint(point1.x, m_graphics.GraphicsDevice.Viewport.Height);
                // Add the vertices
                m_vertsTriStrip[i * 2] = new VertexPositionColor(new Vector3((float)point1.x, (float)point1.y, 0), color);
                m_vertsTriStrip[i * 2 + 1] = new VertexPositionColor(new Vector3((float)point4.x, (float)point4.y, 0), color);
                m_vertsTriStrip[i * 2 + 2] = new VertexPositionColor(new Vector3((float)point2.x, (float)point2.y, 0), color);
                m_vertsTriStrip[i * 2 + 3] = new VertexPositionColor(new Vector3((float)point3.x, (float)point3.y, 0), color);
                // Add the indexes
                m_indexTriStrip[i * 2] = i * 2;
                m_indexTriStrip[i * 2 + 1] = i * 2 + 1;
                m_indexTriStrip[i * 2 + 2] = i * 2 + 2;
                m_indexTriStrip[i * 2 + 3] = i * 2 + 3;
            }
        }

        private void createAllRectangles(List<TPoint> terrainPoints, Color color)
        {
            rectangles = new RectangleE[terrainPoints.Count - 1];
            for (int i = 0; i < terrainPoints.Count - 1; i++)
            {
                var point1 = terrainPoints[i];
                var point2 = terrainPoints[i + 1];
                // Calculate the distance between the two points
                var distance = (float)Math.Sqrt(Math.Pow(point2.x - point1.x, 2) + Math.Pow(point2.y - point1.y, 2));
                // Calculate the angle between the two points
                var angle = (float)Math.Atan2(point2.y - point1.y, point2.x - point1.x);
                // Create a rectangle that will be the line
                var rectangle = new RectangleE(
                    (int)point1.x,
                    (int)point1.y,
                    (int)distance,
                    3,
                    angle, 
                    color);        
                rectangles[i] = rectangle;
            }
        }

        private void DrawTriangleStrip()
        {
            var prevState = m_graphics.GraphicsDevice.RasterizerState;
            m_graphics.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.Solid, MultiSampleAntiAlias= true};
            m_basicEffect.VertexColorEnabled = true;
            m_basicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            m_basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height, 0, 0.1f, 2);
            foreach (var pass in m_basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                m_graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleStrip,
                    m_vertsTriStrip, 0, m_vertsTriStrip.Length,
                    m_indexTriStrip, 0, m_indexTriStrip.Length - 2);
            }
        }
        private void DrawOutline(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < rectangles.Length; i++) { 
                var r = rectangles[i];
                var rectangle = new Rectangle(r.x, r.y, r.width, r.height);
                // Draw the line
                spriteBatch.Draw(m_pixel, rectangle, null, r.color, r.angle, Vector2.Zero, SpriteEffects.None, 0);
            }
        }
    }
}