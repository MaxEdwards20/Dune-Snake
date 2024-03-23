
using Microsoft.Xna.Framework;
using System;

namespace Client.Components
{
    public class Goal : Shared.Components.Component
    {
        public Goal(Vector2 position, float orientation)
        {
            startPosition = position;
            goalPosition = position;
            startOrientation = orientation;
            goalOrientation = orientation;
        }

        public Vector2 startPosition {  get; set; }
        public Vector2 goalPosition { get; set; }
        public float startOrientation { get; set; }
        public float goalOrientation { get; set; }
        public TimeSpan updateWindow {  get; set; }
        public TimeSpan updatedTime { get; set; }
    }
}
