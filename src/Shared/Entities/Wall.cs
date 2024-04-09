using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class Wall
{
    public Entity create(Vector2 position)
    {
        Entity entity = new Entity();
        float size = 10;
        entity.add(new Appearance("Textures/wall"));
        entity.add(new Position(position));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Collision());
        entity.add(new Components.Wall());
        return entity;
    }
}