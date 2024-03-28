namespace Shared.Components;

public class ParentID: Component
{
    public Guid id { get; private set; }
    public ParentID(Guid id)
    {
        this.id = id;
    }
}