using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class Spice
{
    private readonly static Random random = new();
    public static Entity create(Vector2 position)
    {
        int power = random.Next(1, 10);
        float size = 3 * power + 10;
        Entity entity = new();
        entity.add(new Appearance());
        entity.add(new Position(position));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Collidable());
        entity.add(new SpicePower(power));
        return entity;
    }
}