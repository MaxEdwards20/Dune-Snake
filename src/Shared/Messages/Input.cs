
namespace Shared.Messages
{
    public class Input : Message
    {
        public Input(uint entityId, List<Components.Input.Type> inputs, TimeSpan elapsedTime) : base(Messages.Type.Input)
        {
            this.entityId = entityId;
            this.inputs = inputs;
            this.elapsedTime = elapsedTime;
        }

        public Input() : base(Messages.Type.Input)
        {
            this.elapsedTime = TimeSpan.Zero;
            this.inputs = new List<Components.Input.Type>();
        }

        public uint entityId { get; private set; }
        public List<Components.Input.Type> inputs { get; private set; }
        public TimeSpan elapsedTime { get; private set; }

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(entityId));


            data.AddRange(BitConverter.GetBytes(inputs.Count));
            foreach (var input in inputs)
            {
                data.AddRange(BitConverter.GetBytes((UInt16)input));
            }

            data.AddRange(BitConverter.GetBytes(elapsedTime.Milliseconds));

            return data.ToArray();
        }

        public override int parse(byte[] data)
        {
            int offset = base.parse(data);

            this.entityId = BitConverter.ToUInt32(data, offset);
            offset += sizeof(UInt32);

            int howMany = BitConverter.ToInt32(data, offset);
            offset += sizeof(UInt32);

            for (int i = 0; i < howMany; i++)
            {
                var input = (Components.Input.Type)BitConverter.ToUInt16(data, offset);
                offset += sizeof(UInt16);
                inputs.Add(input);
            }

            elapsedTime = new TimeSpan( 0, 0, 0, 0, BitConverter.ToInt32(data, offset));
            offset += sizeof(Int32);

            return offset;
        }
    }
}
