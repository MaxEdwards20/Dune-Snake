using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;
using Shared.Systems;

namespace Shared.Entities
{
    public class WormSegment
    {
        public static Entity create(Vector2 position, float size, float moveRate, float rotateRate, uint parent)
        {
            Entity entity = new Entity();
            entity.add(new Position(position));
            entity.add(new Appearance("Textures/body")); 
            entity.add(new Size(new Vector2(size, size)));
            entity.add(new Movement(moveRate, rotateRate));
            entity.add(new ParentId(parent));
            entity.add(new Collision());
            entity.add(new Worm());
            return entity;
        }
    }
}
