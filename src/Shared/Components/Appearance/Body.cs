using Microsoft.Xna.Framework;

namespace Shared.Components.Appearance;

public class Body: Component
{

    public string texture { get; private set; }
    public Color color { get; private set; }
    public List<Body> bodyParts { get; private set; }= new List<Body>(); 

    public Body(Color color)
    {
        this.texture = "Textures/head"; // TODO: Update this to a real body texture
        this.color = color;
    }
    
    public void addBodyPart(Body bodyPart)
    {
        bodyParts.Add(bodyPart);
    }
    
    public void removeBodyPart(Body bodyPart)
    {
        bodyParts.Remove(bodyPart);
    }
    
    public void clearBodyParts()
    {
        bodyParts.Clear();
    }
}