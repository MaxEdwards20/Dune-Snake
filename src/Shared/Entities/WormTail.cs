using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;
using Shared.Systems;

namespace Shared.Entities;

public class WormTail
{
    
    public static readonly int size = 150;
    public static readonly  float moveRate = 0.3f;
    public static readonly  float rotateRate = (float) Math.PI / 1000;
    public static Entity create(Vector2 position, uint parent, int clientId)
    {
        Entity entity = new Entity();
        entity.add(new Position(position));
        entity.add(new Appearance("Textures/sandworm_tail"));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new ParentId(parent));
        entity.add(new Movement(moveRate, rotateRate));
        entity.add(new ClientId(clientId));
        entity.add(new Collidable());
        entity.add(new Tail());
        entity.add(new AnchorQueue());
        entity.add(new Worm());
        return entity;
    }
}