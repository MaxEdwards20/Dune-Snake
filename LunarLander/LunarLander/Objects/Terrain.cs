// This file will generate terrains Lunar Surface must be procedurally generated, including placement of landing zones; using the random mid-point displacement algorithm. 
// The terrain must be generated in a way that is visually appealing and challenging to the player.
//

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using CS5410.IO;

namespace CS5410.Objects
{
    public class Terrain
    {
        private int landerRadius;
        private int safeZones;
        public  List<TPoint> terrainPoints { get; private set; }
        public List<TPoint> safeZonePoints { get; private set; }
        private TerrainRenderer terrainRenderer;
        private int screenWidth;
        private int screenHeight;
        private Random random;
        private int safeScreenHeightLower;
        private int safeScreenHeightUpper;
        public Terrain(int safeZones, int screenWidth, int screenHeight, int landerRadius)
        {
            this.landerRadius = landerRadius;
            this.safeZones = safeZones;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            safeScreenHeightLower = (int)(screenHeight * .5); // we will render the lander above this
            safeScreenHeightUpper = (int)(screenHeight * .95);
            terrainPoints = new List<TPoint>();
            terrainRenderer = new TerrainRenderer(safeZones);

            random = new Random(); // seed 2 with 9 iterations always misses the second safe zone
        }

        public void loadContent(ContentManager contentManager, GraphicsDeviceManager m_graphics)
        {
            terrainPoints = GenerateTerrain();
            terrainRenderer.loadContent(contentManager, m_graphics, terrainPoints);
        }

        public void render(GameTime gameTime, SpriteBatch spriteBatch)
        {
            terrainRenderer.render(spriteBatch);
        }
        public void reset(int safeZones)
        {
            this.safeZones = safeZones;
            terrainPoints = GenerateTerrain();
            terrainRenderer.newGame(terrainPoints);
        }
        private List<TPoint> GenerateTerrain()
        {
            // Create the initial terrain points. One on each side of the screen
            List<TPoint> points = GenerateStartPoints();
            safeZonePoints = GenerateSafeZones();
            points.AddRange(safeZonePoints);
            points = GenerateTerrainIterative(points);
            return points;
        }

        private List<TPoint> GenerateStartPoints() { 
            var points = new List<TPoint>();
            // we want the height to be randomly between safeScreenHeightLower and safeScreenHeightUpper
            var height1 = random.Next(safeScreenHeightLower, safeScreenHeightUpper);
            var height2 = random.Next(safeScreenHeightLower, safeScreenHeightUpper);
            points.Add(new TPoint(0, height1));
            points.Add(new TPoint(screenWidth, height2));
            return points;
        }

        private List<TPoint> GenerateSafeZones()
        {
            var lengthOfSafeZone = landerRadius * 6;
            var safeZonePoints = new List<TPoint>();
            if (safeZones == 2)
            {
                var zone1 = createSafeZone(10, safeScreenHeightLower, screenWidth / 2, safeScreenHeightUpper, lengthOfSafeZone);
                var zone2 = createSafeZone(screenWidth / 2 + 50, safeScreenHeightLower, screenWidth - 50, safeScreenHeightUpper, lengthOfSafeZone);
                safeZonePoints.AddRange(zone1);
                safeZonePoints.AddRange(zone2);
            }
            else { 
                lengthOfSafeZone = landerRadius * 5; // smaller safe zone when we are on hard mode
                var zone1 = createSafeZone(10, safeScreenHeightLower, screenWidth - 50, safeScreenHeightUpper, lengthOfSafeZone);
                safeZonePoints.AddRange(zone1);
            }
            return safeZonePoints;
        }

        private List<TPoint> createSafeZone(int lowerX, int lowerY, int upperX, int upperY, int lengthOfSafeZone) { 
            var newPoints = new List<TPoint>();
            // Point 1
            var x = random.Next(lowerX, upperX - lengthOfSafeZone);
            var y = random.Next(lowerY, upperY);
            var point1 = new TPoint(x, y, true);
            newPoints.Add(point1);

            // Point 2
            x = (int)(point1.x + lengthOfSafeZone);
            var point2 = new TPoint(x, y, true);
            newPoints.Add(point2);
            return newPoints;
        }



        private List<TPoint> GenerateTerrainIterative(List<TPoint> points)
        {
            var iterations = 7;
            var surfaceRoughness = 1.2;
            while (iterations > 0)
            {
                var pointsCopy = new List<TPoint>(points);
                for (int i = 0; i < pointsCopy.Count - 1; i++)
                {
                    var point1 = pointsCopy[i];
                    var point2 = pointsCopy[i + 1];
                    if (point2.x - point1.x < 1) // less than 1 wont' even be renderable becuase of the rounding
                    {
                        continue;
                    }
                    // Check for safe zone
                    if (!IsInSafeZone(point1, point2))
                    {
                        var newPoint = computeNewPoint(point1, point2, surfaceRoughness);
                        points.Add(newPoint);
                    }
                }
                points.Sort((a, b) => a.x.CompareTo(b.x));
                surfaceRoughness *= Math.Pow(2, -surfaceRoughness);
                iterations--;
            }
            return points;
        }

        private TPoint computeNewPoint(TPoint point1, TPoint point2, double surfaceRoughness)
        {
            var xMidpoint = (point2.x + point1.x) / 2;
            var yMidPoint = (point2.y + point1.y) / 2;
            var r = surfaceRoughness * getGaussianRandomNumber() * Math.Abs(point2.x - point1.x); // This keeps the randomness proporitional to the length of the line
            var newY = yMidPoint + r;
            // Add bounds to the y to keep it within the frame
            if (newY < safeScreenHeightLower)
            {
                newY = safeScreenHeightLower;
            }
            else if (newY > safeScreenHeightUpper)
            {
                newY = safeScreenHeightUpper;
            }
            return new TPoint(xMidpoint, newY);
        }

        private double getGaussianRandomNumber()
        {
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                        Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        private bool IsInSafeZone(TPoint point1, TPoint point2)
        {
            // need point 1 to have a lower x than point 2
            if (point1.x > point2.x)
            {
                var temp = point1;
                point1 = point2;
                point2 = temp;
            }
            if (point1.isPartOfSafeZone && point2.isPartOfSafeZone && point1.y == point2.y)
            {
                return true;
            }
            // iterate over the safe zone points and check if point1 or point2 will cut acorss the safe zone
            for (int i = 0; i < safeZonePoints.Count - 1; i += 2)
            {
                var safePoint1 = safeZonePoints[i];
                var safePoint2 = safeZonePoints[i + 1];
                if (point1.x > safePoint1.x && point1.x < safePoint2.x)
                {
                    return true;
                } else if (point2.x > safePoint1.x && point2.x < safePoint2.x)
                {
                    return true;
                }
            }

            return false;
        }
    }
}