
using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;

namespace Shared.Messages
{
    public class UpdateEntity : Message
    {
        public UpdateEntity(Entity entity, TimeSpan updateWindow) : base(Type.UpdateEntity)
        {
            this.id = entity.id;

            if (entity.contains<Position>())
            {
                this.hasPosition = true;
                this.position = entity.get<Position>().position;
                this.orientation = entity.get<Position>().orientation;
            }

            this.updateWindow = updateWindow;
        }

        public UpdateEntity(): base(Type.UpdateEntity)
        {
        }

        public uint id { get; private set; }

        // Position
        public bool hasPosition { get; private set; } = false;
        public Vector2 position { get; private set; }
        public float orientation { get; private set; }

        // Only the milliseconds are used/serialized
        public TimeSpan updateWindow { get; private set; } = TimeSpan.Zero;

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(id));

            data.AddRange(BitConverter.GetBytes(hasPosition));
            if (hasPosition)
            {
                data.AddRange(BitConverter.GetBytes(position.X));
                data.AddRange(BitConverter.GetBytes(position.Y));
                data.AddRange(BitConverter.GetBytes(orientation));
            }

            data.AddRange(BitConverter.GetBytes((float)updateWindow.TotalMilliseconds));

            return data.ToArray();
        }

        public override int parse(byte[] data)
        {
            int offset = base.parse(data);

            this.id = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);

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

            this.updateWindow = TimeSpan.FromMilliseconds(BitConverter.ToSingle(data, offset));
            offset += sizeof(Single);

            return offset;
        }
    }
}
