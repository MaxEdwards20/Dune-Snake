using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Messages;

public class Collision : Message
{
    public uint senderId { get; private set; }
    public uint receiverId { get; private set; }
    public CollisionType collisionType { get; private set; }
    public bool hasPosition { get; private set; }
    public Vector2 position { get; private set; }
    public float orientation { get; private set; }
    
    public enum CollisionType
    {
        SenderDies,
        ReceiverDies,
        HeadToWall,
        HeadToSpice
    }
    
    public Collision(uint id, uint id2, CollisionType type, Position position) : base(Type.Collision)
    {
        senderId = id;
        receiverId = id2;
        collisionType = type;
        this.position = position.position;
        orientation = position.orientation;
        hasPosition = true;
    }

    public override byte[] serialize()
    {
        List<byte> data = new();
        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(senderId));
        data.AddRange(BitConverter.GetBytes(receiverId));
        data.AddRange(BitConverter.GetBytes((int)collisionType));
        serializePosition(data);
        return data.ToArray();
    }
    
    public override int parse(byte[] data)
    {
        int offset = base.parse(data);
        senderId = BitConverter.ToUInt32(data, offset);
        offset += sizeof(uint);
        receiverId = BitConverter.ToUInt32(data, offset);
        offset += sizeof(uint);
        collisionType = (CollisionType)BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        offset = parsePosition(data, offset);
        return offset;
    }
    
    private void serializePosition(List<byte> data)
    {
        data.AddRange(BitConverter.GetBytes(hasPosition));
        if (hasPosition)
        {
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));
            data.AddRange(BitConverter.GetBytes(orientation));
        }
    }
    
    
    private int parsePosition(byte[] data, int offset)
    {
        this.hasPosition = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasPosition)
        {
            float positionX = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            float positionY = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            this.position = new Vector2(positionX, positionY);
            this.orientation = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
        }

        return offset;
    }
    

    public Collision() : base(Type.Collision)
    {
    }
    
}