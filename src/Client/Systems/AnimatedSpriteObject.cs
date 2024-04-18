using Microsoft.Xna.Framework;

namespace CS5410.Objects
{
    public class AnimatedSprite
    {
        private readonly Vector2 m_size;
        protected Vector2 m_center;
        protected float m_rotation = 0;

        public AnimatedSprite(Vector2 size, Vector2 center)
        {
            m_size = size;
            m_center = center;
        }

        public Vector2 Size
        {
            get { return m_size; }
        }

        public Vector2 Center
        {
            get { return m_center; }
        }

        public float Rotation
        {
            get { return m_rotation; }
        }
    }
}
