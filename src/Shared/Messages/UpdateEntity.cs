
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

            if (entity.contains<SpicePower>())
            {
                this.spicePower = entity.get<SpicePower>().power;
            }
            
            if (entity.contains<ParentId>())
            {
                this.hasParent = true;
                this.parentId = entity.get<ParentId>().id;
            }
            
            if (entity.contains<ChildId>())
            {
                this.hasChild = true;
                this.childId = entity.get<ChildId>().id;
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
        
        // SpicePower
        public bool hasSpicePower { get; private set; } = false;
        public int spicePower { get; private set; } = 0;
        public bool hasParent { get; private set; } = false;
        public uint parentId { get; private set; }
        public bool hasChild { get; private set; } = false;
        public uint childId { get; private set; }

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
            
            data.AddRange(BitConverter.GetBytes(hasSpicePower));
            if (hasSpicePower)
            {
                data.AddRange(BitConverter.GetBytes(spicePower));
            }
            serializeParent(data);
            serializeChild(data);

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
            
            this.hasSpicePower = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasSpicePower)
            {
                this.spicePower = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            offset = parseParent(data, offset);
            offset = parseChild(data, offset);

            this.updateWindow = TimeSpan.FromMilliseconds(BitConverter.ToSingle(data, offset));
            offset += sizeof(Single);

            return offset;
        }
        
        private int parseParent(byte[] data, int offset)
        {
            this.hasParent = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasParent)
            {
                this.parentId = BitConverter.ToUInt32(data, offset);
                offset += sizeof(uint);
            }

            return offset;
        }

        private int parseChild(byte[] data, int offset)
        {
            this.hasChild = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasChild)
            {
                this.childId = BitConverter.ToUInt32(data, offset);
                offset += sizeof(uint);
            }
            return offset;
        }
        
        private void serializeChild(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasChild));
            if (hasChild)
            {
                data.AddRange(BitConverter.GetBytes(childId));
            }
        }

        private void serializeParent(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasParent));
            if (hasParent)
            {
                data.AddRange(BitConverter.GetBytes(parentId));
            }
        }
    }
}
