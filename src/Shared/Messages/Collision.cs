namespace Shared.Messages;

public class Collision : Message
{
    public uint entity1Id { get; private set; }
    public uint entity2Id { get; private set; }
    
    public Collision(uint id, uint id2) : base(Type.Collision)
    {
        entity1Id = id;
        entity2Id = id2;
    }
    
    public Collision() : base(Type.Collision)
    {
    }
    
    
}