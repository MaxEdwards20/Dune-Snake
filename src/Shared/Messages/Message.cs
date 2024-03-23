
namespace Shared.Messages
{
    public abstract class Message
    {
        public Message(Type type)
        {
            this.type = type;
            this.messageId = null;
        }

        public Type type { get; private set; }
        public uint? messageId { get; set; }

        public virtual byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(this.messageId.HasValue));
            if (this.messageId.HasValue)
            {
                data.AddRange(BitConverter.GetBytes((uint)this.messageId.Value));
            }

            return data.ToArray();
        }
        /// <summary>
        /// Returns the number of bytes parsed
        /// </summary>
        public virtual int parse(byte[] data)
        {
            int offset = 0;

            bool hasValue = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasValue)
            {
                this.messageId = BitConverter.ToUInt32(data, offset);
                offset += sizeof(uint);
            }

            return offset;
        }
    }
}
