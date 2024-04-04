using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class Spice
{
    private Random random = new Random();
    public Entity create(Color color, Vector2 position)
    {
        int power = random.Next(1, 10);
        float size = 10;
        Entity entity = new Entity();
        entity.add(new Appearance("Textures/spice")); // TODO: Make this a spice texture
        entity.add(new Position(position));
        entity.add(new Size(new Vector2(size * power, size * power)));
        entity.add(new Collision());
        entity.add(new SpicePower(power));
        return entity;
    }
}