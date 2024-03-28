using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class Spice
{
    public Entity create(Color color, Vector2 position, float size)
    {
        Entity entity = new Entity();
        entity.add(new Appearance("Textures/spice")); // TODO: Make this a spice texture
        entity.add(new Position(position));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Collision());
        return entity;
    }
}