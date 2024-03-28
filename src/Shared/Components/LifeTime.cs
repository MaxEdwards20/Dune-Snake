namespace Shared.Components;

public class LifeTime: Component
{
    public TimeSpan lifeTime { get; private set; }
    public LifeTime(TimeSpan lifeTime)
    {
        this.lifeTime = lifeTime;
    }
    
    public void update(TimeSpan elapsedTime)
    {
        lifeTime -= elapsedTime;
    }
}