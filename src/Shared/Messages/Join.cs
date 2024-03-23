
namespace Shared.Messages
{
    public class Join : Message
    {
        public Join() : base(Type.Join)
        {

        }

        /// <summary>
        /// In this case, the message type is all we need, so just sending a single
        /// byte of empty data as the message body.
        /// </summary>
        public override byte[] serialize()
        {
            return base.serialize();
        }

        /// <summary>
        /// Don't actually need to parse anything, as the message body is just a
        /// dummy byte.
        /// </summary>
        public override int parse(byte[] data)
        {
            return base.parse(data);
        }
    }
}
