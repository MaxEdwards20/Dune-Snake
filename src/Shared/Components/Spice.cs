using Microsoft.Xna.Framework;
namespace Shared.Components;
public class Spice
{
    public int spiceLevel { get; private set; }
    public Color color { get; private set; }
    public string texture { get; private set; }
    
    public Spice(int spiceLevel, Color color)
    {
        this.spiceLevel = spiceLevel;
        this.color = color;
        this.texture = "Textures/spice"; // TODO: Update this to a real spice texture
    }
}