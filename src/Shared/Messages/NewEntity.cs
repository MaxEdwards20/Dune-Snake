using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;
using System.Text;
using Shared.Components.Appearance;

namespace Shared.Messages
{
    public class NewEntity : Message
    {
        public NewEntity(Entity entity) : base(Type.NewEntity)
        {
            this.id = entity.id;

            if (entity.contains<Appearance>())
            {
                this.hasAppearance = true;
            }
            else
            {
                this.texture = "";
            }

            if (entity.contains<Position>())
            {
                this.hasPosition = true;
                this.position = entity.get<Position>().position;
                this.orientation = entity.get<Position>().orientation;
            }

            if (entity.contains<Size>())
            {
                this.hasSize = true;
                this.size = entity.get<Size>().size;
            }

            if (entity.contains<Movement>())
            {
                this.hasMovement = true;
                this.moveRate = entity.get<Movement>().moveRate;
                this.rotateRate = entity.get<Movement>().rotateRate;
            }
            
            if (entity.contains<ClientId>())
            {
                this.hasClientId = true;
                this.clientId = entity.get<ClientId>().m_id;
            }

            if (entity.contains<Components.Input>())
            {
                this.hasInput = true;
                this.inputs = entity.get<Components.Input>().inputs;
            }
            else
            {
                this.inputs = new List<Components.Input.Type>();
            }

            // Worm parts
            if (entity.contains<Head>())
            {
                this.hasHead = true;
            }

            if (entity.contains<Tail>())
            {
                this.hasTail = true;
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

            if (entity.contains<Shared.Components.Collidable>())
            {
                this.hasCollision = true;
            }

            if (entity.contains<Worm>())
            {
                this.hasWorm = true;
            }

            if (entity.contains<Name>())
            {
                this.hasName = true;
                this.name = entity.get<Name>().name;
            }

            if (entity.contains<Stats>())
            {
                HasStats = true;
                Stats stats = entity.get<Stats>();
                Kills = stats.Kills;
                Score = stats.Score;
            }

            if (entity.contains<Shared.Components.Wall>())
            {
                this.hasWall = true;
            }

            if (entity.contains<Shared.Components.Invincible>())
            {
                this.hasInvincible = true;
                this.invincibleDuration = entity.get<Shared.Components.Invincible>().duration;
            }

            if (entity.contains<Shared.Components.SpicePower>())
            {
                this.hasSpicePower = true;
                this.spicePower = entity.get<Shared.Components.SpicePower>().power;
            }
        }
        public NewEntity() : base(Type.NewEntity)
        {
            this.texture = "";
            this.inputs = new List<Components.Input.Type>();
        }

        public uint id { get; private set; }
        public bool HasStats { get; private set; } = false;
        public uint Kills { get; private set; }
        public uint Score { get; private set; }

        // Appearance
        public bool hasAppearance { get; private set; } = false;
        public string texture { get; private set; }
        
        public bool hasClientId { get; private set; } = false;
        public int clientId { get; private set; }
        public bool hasCollision { get; private set; } = false;
        public bool hasWall { get; private set; } = false;

        // Worm parts
        public bool hasHead { get; private set; } = false;
        public bool hasTail { get; private set; } = false;
        public bool hasParent { get; private set; } = false;
        public uint parentId { get; private set; }
        public bool hasChild { get; private set; } = false;
        public uint childId { get; private set; }
        public bool hasWorm { get; private set; } = false;
        public bool hasName { get; private set; } = false;
        public string name { get; private set; }


        // Position
        public bool hasPosition { get; private set; } = false;
        public Vector2 position { get; private set; }
        public float orientation { get; private set; }

        // Size
        public bool hasSize { get; private set; } = false;
        public Vector2 size { get; private set; }

        // Movement
        public bool hasMovement { get; private set; } = false;
        public float moveRate { get; private set; }
        public float rotateRate { get; private set; }

        // Input
        public bool hasInput { get; private set; } = false;
        public List<Components.Input.Type> inputs { get; private set; }

        // Invincible
        public bool hasInvincible { get; private set; } = false;
        public int invincibleDuration { get; private set; }

        // SpicePower
        public bool hasSpicePower { get; private set; } = false;
        public int spicePower { get; private set; }

        public override byte[] serialize()
        {
            List<byte> data = new();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(id));

            serializeStats(data);
            serializeAppearance(data);
            serializePosition(data);
            serializeSize(data);
            serializeMovement(data);
            serializeInput(data);
            serializeClientId(data);
            data.AddRange(BitConverter.GetBytes(hasCollision));
            data.AddRange(BitConverter.GetBytes(hasWall));

            // Worm entities
            data.AddRange(BitConverter.GetBytes(hasHead));
            data.AddRange(BitConverter.GetBytes(hasTail));
            serializeParentId(data);
            serializeChild(data);
            data.AddRange(BitConverter.GetBytes(hasWorm));
            serializeInvincible(data);
            serializeSpicePower(data);
            serializeName(data); // Make sure this is the last item to serialize

            return data.ToArray();
        }

        private void serializeClientId(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasClientId));
            if (hasClientId)
            {
                data.AddRange(BitConverter.GetBytes(clientId));
            }
        }

        public override int parse(byte[] data)
        {
            // NOTE: Add parser for the components on the WormHead, WormSegment, and WormTail entities

            // Upgrade: Could move all of these methods to the associated components
            int offset = base.parse(data);

            offset = parseId(data, offset);
            offset = parseStats(data, offset);
            offset = parseAppearance(data, offset);
            offset = parsePosition(data, offset);
            offset = parseSize(data, offset);
            offset = parseMovement(data, offset);
            offset = parseInput(data, offset);
            offset = parseClientId(data, offset);
            offset = parseCollision(data, offset);
            offset = parseWall(data, offset);

            // Worm Entities
            offset = parseHead(data, offset);
            offset = parseTail(data, offset);
            offset = parseParent(data, offset);
            offset = parseChild(data, offset);
            offset = parseWorm(data, offset);
            offset = parseInvincible(data, offset);
            offset = parseSpicePower(data, offset);
            offset = parseName(data, offset); // Make sure this is the last item to parse
            return offset;
        }
        
        private int parseClientId(byte[] data, int offset)
        {
            this.hasClientId = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasClientId)
            {
                this.clientId = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }

            return offset;
        }

        private int parseStats(byte[] data, int offset)
        {
            HasStats = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (HasStats)
            {
                Kills = BitConverter.ToUInt32(data, offset);
                offset += sizeof(int);
                Score = BitConverter.ToUInt32(data, offset);
                offset += sizeof(int);
            }

            return offset;
        }

        private int parseSpicePower(byte[] data, int offset)
        {
            this.hasSpicePower = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasSpicePower)
            {
                this.spicePower = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }

            return offset;
        }

        private int parseInvincible(byte[] data, int offset)
        {
            this.hasInvincible = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasInvincible)
            {
                this.invincibleDuration = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }

            return offset;
        }

        private int parseWorm(byte[] data, int offset)
        {
            this.hasWorm = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            return offset;
        }

        private int parseWall(byte[] data, int offset)
        {
            this.hasWall = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            return offset;
        }

        private int parseId(byte[] data, int offset)
        {
            this.id = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);
            return offset;
        }

        private int parseTail(byte[] data, int offset)
        {
            this.hasTail = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            return offset;
        }

        private int parseHead(byte[] data, int offset)
        {
            this.hasHead = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
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

        private int parseName(byte[] data, int offset)
        {
            this.hasName = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasName)
            {
                // int nameSize = BitConverter.ToInt32(data, offset);
                // offset += sizeof(Int32);
                int nameSize = data.Length - offset;
                this.name = Encoding.UTF8.GetString(data, offset, nameSize);
                offset += nameSize;
            }
            return offset;
        }

        private int parseCollision(byte[] data, int offset)
        {
            this.hasCollision = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            return offset;
        }

        private int parseInput(byte[] data, int offset)
        {
            this.hasInput = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasInput)
            {
                int howMany = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                for (int i = 0; i < howMany; i++)
                {
                    inputs.Add((Components.Input.Type) BitConverter.ToUInt16(data, offset));
                    offset += sizeof(UInt16);
                }
            }

            return offset;
        }

        private int parseMovement(byte[] data, int offset)
        {
            this.hasMovement = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasMovement)
            {
                this.moveRate = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                this.rotateRate = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
            }

            return offset;
        }

        private int parseSize(byte[] data, int offset)
        {
            this.hasSize = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasSize)
            {
                float sizeX = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                float sizeY = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                this.size = new Vector2(sizeX, sizeY);
            }

            return offset;
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

        private int parseAppearance(byte[] data, int offset)
        {
            this.hasAppearance = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            return offset;
        }

        private void serializeSpicePower(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasSpicePower));
            if (hasSpicePower)
            {
                data.AddRange(BitConverter.GetBytes(spicePower));
            }
        }

        private void serializeInvincible(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasInvincible));
            if (hasInvincible)
            {
                data.AddRange(BitConverter.GetBytes(invincibleDuration));
            }
        }
        private void serializeChild(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasChild));
            if (hasChild)
            {
                data.AddRange(BitConverter.GetBytes(childId));
            }
        }

        private void serializeParentId(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasParent));
            if (hasParent)
            {
                data.AddRange(BitConverter.GetBytes(parentId));
            }
        }

        private void serializeName(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasName));
            if (hasName)
            {
                data.AddRange(Encoding.UTF8.GetBytes(name));
            }
        }

        private void serializeStats(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(HasStats));
            if (HasStats)
            {
                data.AddRange(BitConverter.GetBytes(Score));
                data.AddRange(BitConverter.GetBytes(Kills));
            }
        }

        private void serializeInput(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasInput));
            if (hasInput)
            {
                data.AddRange(BitConverter.GetBytes(inputs.Count));
                foreach (var input in inputs)
                {
                    data.AddRange(BitConverter.GetBytes((UInt16) input));
                }
            }
        }

        private void serializeMovement(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasMovement));
            if (hasMovement)
            {
                data.AddRange(BitConverter.GetBytes(moveRate));
                data.AddRange(BitConverter.GetBytes(rotateRate));
            }
        }

        private void serializeSize(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasSize));
            if (hasSize)
            {
                data.AddRange(BitConverter.GetBytes(size.X));
                data.AddRange(BitConverter.GetBytes(size.Y));
            }
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

        private void serializeAppearance(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasAppearance));
        }
    }
}

