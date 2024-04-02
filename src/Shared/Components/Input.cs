
namespace Shared.Components
{
    public class Input : Component
    {
        public enum Type : UInt16
        {
            SnakeUp,
            RotateLeft,
            RotateRight,
        }

        public Input(List<Type> inputs)
        {
            this.inputs = inputs;
        }

        public List<Type> inputs { get; private set; }
    }
}
