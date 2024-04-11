using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class DeadWormSpice
{
    private readonly static Random random = new();
    public static Entity create(Vector2 position)
    {
        float size = 50;
        Entity entity = new();
        entity.add(new Appearance("Textures/deadWormSpice"));
        entity.add(new Position(position));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Collidable());
        entity.add(new SpicePower(10));
        return entity;
    }
}