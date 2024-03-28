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
                this.texture = entity.get<Appearance>().texture;
            }
            else
            {
                this.texture = "";
            }
            handleWormAppearance(entity);            
            
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

            if (entity.contains<Components.Input>())
            {
                this.hasInput = true;
                this.inputs = entity.get<Components.Input>().inputs;
            }
            else
            {
                this.inputs = new List<Components.Input.Type>();
            }
        }
        public NewEntity() : base(Type.NewEntity)
        {
            this.texture = "";
            this.inputs = new List<Components.Input.Type>();
        }

        public uint id { get; private set; }

        // Appearance
        public bool hasAppearance { get; private set; } = false;
        public string texture { get; private set; }
        
        // Worm Appearance
        public bool hasHead { get; private set; } = false;
        public string textureHead { get; private set; }
        public bool hasBody { get; private set; } = false;
        public string textureBody { get; private set; }
        public bool hasTail { get; private set; } = false;
        public string textureTail { get; private set; }

        // Position
        public bool hasPosition { get; private set; } = false;
        public Vector2 position { get; private set; }
        public float orientation { get; private set; }
        
        // size
        public bool hasSize { get; private set; } = false;
        public Vector2 size { get; private set; }

        // Movement
        public bool hasMovement { get; private set; } = false;
        public float moveRate { get; private set; }
        public float rotateRate { get; private set; }

        // Input
        public bool hasInput { get; private set; } = false;
        public List<Components.Input.Type> inputs { get; private set; }

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(id));

            data.AddRange(BitConverter.GetBytes(hasAppearance));
            if (hasAppearance)
            {
                data.AddRange(BitConverter.GetBytes(texture.Length));
                data.AddRange(Encoding.UTF8.GetBytes(texture));
            }
            
            serializeWormAppearance(data);

            data.AddRange(BitConverter.GetBytes(hasPosition));
            if (hasPosition)
            {
                data.AddRange(BitConverter.GetBytes(position.X));
                data.AddRange(BitConverter.GetBytes(position.Y));
                data.AddRange(BitConverter.GetBytes(orientation));
            }

            data.AddRange(BitConverter.GetBytes(hasSize));
            if (hasSize)
            {
                data.AddRange(BitConverter.GetBytes(size.X));
                data.AddRange(BitConverter.GetBytes(size.Y));
            }

            data.AddRange(BitConverter.GetBytes(hasMovement));
            if (hasMovement)
            {
                data.AddRange(BitConverter.GetBytes(moveRate));
                data.AddRange(BitConverter.GetBytes(rotateRate));
            }

            data.AddRange(BitConverter.GetBytes(hasInput));
            if (hasInput)
            {
                data.AddRange(BitConverter.GetBytes(inputs.Count));
                foreach (var input in inputs)
                {
                    data.AddRange(BitConverter.GetBytes((UInt16)input));
                }
            }

            return data.ToArray();
        }



        public override int parse(byte[] data)
        {
            int offset = base.parse(data);

            this.id = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);

            this.hasAppearance = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasAppearance)
            {
                int textureSize = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.texture = Encoding.UTF8.GetString(data, offset, textureSize);
                offset += textureSize;
            }
            
            offset += deserializeWormAppearance(data, offset);

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

            this.hasMovement = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasMovement)
            {
                this.moveRate = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                this.rotateRate = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
            }

            this.hasInput = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasInput)
            {
                int howMany = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                for (int i = 0; i < howMany; i++)
                {
                    inputs.Add((Components.Input.Type)BitConverter.ToUInt16(data, offset));
                    offset += sizeof(UInt16);
                }
            }

            return offset;
        }
        
        private void handleWormAppearance(Entity entity)
        {
            if (entity.contains<Head>())
            {
                this.hasHead = true;
                this.textureHead = entity.get<Head>().color.ToString();
            }
            else
            {
                this.textureHead = "";
            }

            if (entity.contains<Body>())
            {
                this.hasBody = true;
                this.textureBody = entity.get<Body>().color.ToString();
            }
            else
            {
                this.textureBody = "";
            }

            if (entity.contains<Tail>())
            {
                this.hasTail = true;
                this.textureTail = entity.get<Tail>().color.ToString();
            }
            else
            {
                this.textureTail = "";
            }
        }
        
        
        private void serializeWormAppearance(List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(hasHead));
            if (hasHead)
            {
                data.AddRange(BitConverter.GetBytes(textureHead.Length));
                data.AddRange(Encoding.UTF8.GetBytes(textureHead));
            }

            data.AddRange(BitConverter.GetBytes(hasBody));
            if (hasBody)
            {
                data.AddRange(BitConverter.GetBytes(textureBody.Length));
                data.AddRange(Encoding.UTF8.GetBytes(textureBody));
            }

            data.AddRange(BitConverter.GetBytes(hasTail));
            if (hasTail)
            {
                data.AddRange(BitConverter.GetBytes(textureTail.Length));
                data.AddRange(Encoding.UTF8.GetBytes(textureTail));
            }
        }
        
        
        private int deserializeWormAppearance(byte[] data, int offset)
        {
            this.hasHead = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasHead)
            {
                int textureSize = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.textureHead = Encoding.UTF8.GetString(data, offset, textureSize);
                offset += textureSize;
            }
            
            this.hasBody = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasBody)
            {
                int textureSize = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.textureBody = Encoding.UTF8.GetString(data, offset, textureSize);
                offset += textureSize;
            }
            
            this.hasTail = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasTail)
            {
                int textureSize = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                this.textureTail = Encoding.UTF8.GetString(data, offset, textureSize);
                offset += textureSize;
            }

            return offset;
        }
    }
    
    
}
