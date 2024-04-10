using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class WormHead
{
    static readonly int size = 100; 
    static readonly  float moveRate = 0.3f;
    static readonly  float rotateRate = (float) Math.PI / 1000;
    public static Entity create(Vector2 position, string name)
    {
        Entity entity = new Entity();
        entity.add(new Position(position));
        entity.add(new Appearance("Textures/circleHead"));
        entity.add(new Size(new Vector2(size, size)));
        entity.add(new Movement(moveRate, rotateRate));
        entity.add(new Collision());
        entity.add(new SpicePower(0));
        entity.add(new Head());
        entity.add(new Name(name));
        entity.add(new Worm());
            
        List<Input.Type> inputs = new List<Input.Type>();
        foreach (Shared.Components.Input.Type input in Enum.GetValues(typeof(Input.Type)))
        {
            inputs.Add(input);
        }
        entity.add(new Input(inputs));

        return entity;
    }
    
}

