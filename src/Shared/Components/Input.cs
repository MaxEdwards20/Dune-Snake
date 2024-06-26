﻿
namespace Shared.Components
{
    public class Input : Component
    {
        public enum Type : UInt16
        {
            PointLeft,
            PointRight,
            PointUp,
            PointDown,
            PointUpLeft,
            PointUpRight,
            PointDownLeft,
            PointDownRight,
        }

        public Input(List<Type> inputs)
        {
            this.inputs = inputs;
        }

        public List<Type> inputs { get; private set; }
    }
}
