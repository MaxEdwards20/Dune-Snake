using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class WormSegment
{
    static readonly int size = 80; 
    static readonly  float moveRate = 0.3f;
    static readonly  float rotateRate = (float) Math.PI / 1000;
    public static Entity create(Vector2 position, uint parent)
    {
        Entity entity = new Entity();
        entity.add(new Position(position));
        entity.add(new Appearance("Textures/circleBody")); 
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Movement(moveRate, rotateRate));
        entity.add(new ParentId(parent));
        entity.add(new Collision());
        entity.add(new Worm());
        entity.add(new AnchorQueue());
        return entity;
    }
}

