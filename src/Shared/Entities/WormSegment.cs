using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class WormSegment
{
    static readonly int size = 150; 
    static readonly  float moveRate = 0.3f;
    static readonly  float rotateRate = (float) Math.PI / 1000;
    public static Entity create(Vector2 position, uint parent, int clientId)
    {
        Entity entity = new Entity();
        entity.add(new Position(position));
        entity.add(new Appearance()); 
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Movement(moveRate, rotateRate));
        entity.add(new ClientId(clientId));
        entity.add(new ParentId(parent));
        entity.add(new Collidable());
        entity.add(new Worm());
        entity.add(new AnchorQueue());
        return entity;
    }
}

