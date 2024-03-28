using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

// This is the Player. We only move the head, all the other parts of the snake follow.
public class WormHead
{
    public static Entity create(Color color, Vector2 position, float size, float moveRate, float rotateRate)
    {
        Entity entity = new Entity();
        entity.add(new SegmentID());
        entity.add(new Position(position));
        entity.add(new Appearance("Textures/head"));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Movement(moveRate, rotateRate));
        entity.add(new Collision());
        entity.add(new SpicePower(0));
            
        List<Input.Type> inputs = new List<Input.Type>();
        inputs.Add(Input.Type.SnakeUp);
        inputs.Add(Input.Type.RotateLeft);
        inputs.Add(Input.Type.RotateRight);
        inputs.Add(Input.Type.SnakeDown);
        inputs.Add(Input.Type.Boost);
        entity.add(new Input(inputs));

        return entity;
    }

    
}

