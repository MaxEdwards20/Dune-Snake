using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Messages;

public class NewAnchorPoint :Message
{
    
    public NewAnchorPoint(Position position, uint wormHeadId) : base(Type.NewAnchorPoint)
    {
        this.wormHeadId = wormHeadId;
        this.position = position.position;
        this.orientation = position.orientation;
    }
    
    public uint wormHeadId { get; private set; }
    public Vector2 position { get; private set; }
    public float orientation { get; private set; }

    public override byte[] serialize()
    {
        List<byte> data = new List<byte>();

        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(wormHeadId));
        data.AddRange(BitConverter.GetBytes(position.X));
        data.AddRange(BitConverter.GetBytes(position.Y));
        data.AddRange(BitConverter.GetBytes(orientation));

        return data.ToArray();
    }
    
    public override int parse(byte[] data)
    {
        int offset = base.parse(data);

        this.wormHeadId = BitConverter.ToUInt32(data, offset);
        offset += sizeof(UInt32);
        this.position = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)));
        offset += sizeof(float) * 2;
        this.orientation = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        return offset ;
    }


    public NewAnchorPoint(Type type) : base(type)
    {
    }
}