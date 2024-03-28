using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;
using Shared.Systems;

namespace Shared.Entities;

public class WormTail
{
    public static Entity create(Color color, Vector2 position, float size, float moveRate, float rotateRate, Guid parent)
    {
        Entity entity = new Entity();
        entity.add(new SegmentID());
        entity.add(new ParentID(parent));
        entity.add(new Position(position));
        entity.add(new Appearance("Textures/tail")); 
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Movement(moveRate, rotateRate));
        entity.add(new Collision());
        return entity;
    }
}