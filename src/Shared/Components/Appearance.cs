
namespace Shared.Components.Appearance
{
    public class Appearance : Component
    {
        public Appearance(string texture)
        {
            this.texture = texture;
        }
        public string texture { get; private set; }
    }
}
