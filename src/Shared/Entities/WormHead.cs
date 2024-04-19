using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class WormHead
{
    static readonly int size = 150;
    static readonly float moveRate = 0.3f;
    static readonly float rotateRate = (float)Math.PI / 1000;

    public static Entity create(Vector2 position, string name, int clientId)
    {
        Entity entity = new();
        entity.add(new Position(position));
        entity.add(new Appearance());
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Movement(moveRate, rotateRate));
        entity.add(new ClientId(clientId));
        entity.add(new Collidable());
        entity.add(new SpicePower(0));
        entity.add(new Head());
        entity.add(new Name(name));
        entity.add(new Worm());
        entity.add(new Stats(0, 0));

        List<Input.Type> inputs = new();
        foreach (Input.Type input in Enum.GetValues(typeof(Input.Type)))
            inputs.Add(input);

        entity.add(new Input(inputs));

        return entity;
    }
}

