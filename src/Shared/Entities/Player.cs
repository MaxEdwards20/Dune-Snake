using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;
using Shared.Systems;

namespace Shared.Entities
{
    public class Player
    {
        public static Entity create(Color color, Vector2 position, float size, float moveRate, float rotateRate)
        {
            Entity entity = new Entity();
            entity.add(new Position(position));
            entity.add(new Size(new Vector2(size, size)));
            entity.add(new Movement(moveRate, rotateRate));
            
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
}
