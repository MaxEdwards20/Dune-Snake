    namespace CS5410.Objects
       {
            public class TPoint
            {
                public double x;
                public double y;
                public bool isPartOfSafeZone { get; private set; }
                public TPoint(double x, double y, bool isPartOfSafeZone = false)
                {
                    this.x = x;
                    this.y = y;
                    this.isPartOfSafeZone = isPartOfSafeZone;
                }
        public override string ToString()
        {
            return "X: " + x + " Y: " + y;
        }
    }
       } 
