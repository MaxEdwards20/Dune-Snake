namespace Shared.Components;

public class SegmentID : Component
{
    public Guid id { get; private set; }

    public SegmentID()
    {
        this.id = Guid.NewGuid();
    }
    
}