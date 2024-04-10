namespace Shared.Components;

public class Invincible: Component
{
    public int duration { get; private set; }
    public Invincible(int duration = 3000)
    {
        this.duration = duration;
    }
    
    public void update(int elapsedTime)
    {
        duration -= elapsedTime;
    }
    
}