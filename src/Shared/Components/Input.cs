
namespace Shared.Components
{
    public class Input : Component
    {
        public enum Type : UInt16
        {
            SnakeUp,
            SnakeDown,
            RotateLeft,
            RotateRight,
            Boost,
            FollowMouse // for following the mouse cursor
        }

        public Input(List<Type> inputs)
        {
            this.inputs = inputs;
        }

        public List<Type> inputs { get; private set; }
    }
}
