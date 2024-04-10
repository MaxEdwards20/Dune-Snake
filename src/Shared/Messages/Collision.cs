namespace Shared.Messages;

public class Collision : Message
{
    public uint entity1Id { get; private set; }
    public uint entity2Id { get; private set; }
    public CollisionType collisionType { get; private set; }
    
    public enum CollisionType
    {
        HeadToHead,
        HeadToBody,
        HeadToWall,
        HeadToSpice
    }
    
    public Collision(uint id, uint id2, CollisionType type) : base(Type.Collision)
    {
        entity1Id = id;
        entity2Id = id2;
        collisionType = type;
    }
    
    public Collision() : base(Type.Collision)
    {
    }
    
    
}