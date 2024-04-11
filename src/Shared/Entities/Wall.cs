using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class Wall
{
    public static Entity create(Vector2 position, int size)
    {
        Entity entity = new Entity();
        entity.add(new Appearance("Textures/wall"));
        entity.add(new Position(position));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Collidable());
        entity.add(new Components.Wall());
        return entity;
    }
}