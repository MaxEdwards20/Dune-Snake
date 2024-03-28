using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;
using Shared.Systems;

namespace Shared.Entities
{
    public class OtherWorm
    {
        public static Entity create(Color color, Vector2 position, float size, float moveRate, float rotateRate)
        {
            Entity entity = new Entity();
            entity.add(new Head(color));
            entity.add(new Body(color));
            entity.add(new Tail(color));
            entity.add(new Position(position));
            entity.add(new Size(new Vector2(size, size)));
            entity.add(new Movement(moveRate, rotateRate));

            return entity;
        }
    }
}
