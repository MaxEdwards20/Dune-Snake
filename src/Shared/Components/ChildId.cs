namespace Shared.Components;

public class ChildId : Component
{
    public uint id { get; private set; }
    public ChildId(uint id)
    {
        this.id = id;
    }
    
    public void setId(uint id)
    {
        this.id = id;
    }
    
}