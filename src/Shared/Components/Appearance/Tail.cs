using Microsoft.Xna.Framework;

namespace Shared.Components.Appearance;

public class Tail : Component
{
    public Tail(Color color)
    {
        this.color = color;
        this.texture = "Textures/head"; // TODO Update this to a real tail texture
    }
    public string texture { get; private set; }
    public Color color { get; private set; }
}