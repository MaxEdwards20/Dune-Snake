
namespace Shared.Messages
{
    public class RemoveEntity : Message
    {
        public RemoveEntity() : base(Type.RemoveEntity)
        {
        }

        public RemoveEntity(uint id) : base(Type.RemoveEntity)
        {
            this.id = id;
        }

        public uint id { get; private set; }

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(id));

            return data.ToArray();
        }

        public override int parse(byte[] data)
        {
            int offset = base.parse(data);

            this.id = BitConverter.ToUInt32(data, offset);
            offset += sizeof(UInt32);

            return offset;
        }
    }
}
