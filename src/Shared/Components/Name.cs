namespace Shared.Components;

public class Name: Component
{
    public string name { get; private set; }
    
    public Name(string name)
    {
        this.name = name;
    }
}