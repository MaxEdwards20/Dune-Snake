
using System.Text;

namespace Shared.Messages
{
    public class Join : Message
    {
        public Join(string name) : base(Type.Join)
        {
            this.name = name;
            this.hasName = true;
        }
        
        public Join() : base(Type.Join)
        {
            this.hasName = false;
        }
        
        public bool hasName { get; private set; } = false;
        public string name { get; private set; }

        /// <summary>
        /// In this case, the message type is all we need, so just sending a single
        /// byte of empty data as the message body.
        /// </summary>
        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(hasName));
            if (hasName)
            {
                data.AddRange(Encoding.UTF8.GetBytes(name));
            }
            return data.ToArray();
        }

        /// <summary>
        /// Don't actually need to parse anything, as the message body is just a
        /// dummy byte.
        /// </summary>
        public override int parse(byte[] data)
        {
            int offset = base.parse(data);
            this.hasName = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasName)
            {
                int nameSize = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.name = Encoding.UTF8.GetString(data, offset, nameSize);
                offset += nameSize;
            }
            return offset;
        }
    }
}
