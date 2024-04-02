namespace Shared.Components;

public class ParentId: Component
{
    public uint id { get; private set; }
    public ParentId(uint id)
    {
        this.id = id;
    }
}