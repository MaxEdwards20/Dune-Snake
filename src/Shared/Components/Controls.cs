using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Shared.Components
{
    [DataContract(Name = "Controls")]
    public class Controls: Component
    {
        [DataMember(Name = "SnakeUp")]
        public Control SnakeUp = new Control(Keys.Up);
        [DataMember(Name = "SnakeLeft")]
        public Control SnakeLeft = new Control(Keys.Left);
        [DataMember(Name = "SnakeRight")]
        public Control SnakeRight = new Control(Keys.Right);
        [DataMember(Name = "SnakeDown")]
        public Control SnakeDown = new Control(Keys.Down);
        [DataMember (Name = "SnakeBoost")]
        public Control SnakeBoost = new Control(Keys.Space);
    }
    
    public class Control
    {
        [DataMember(Name = "key")]
        public Keys key { get; private set; }

        public Control(Keys key)
        {
            this.key = key;
        }

        public void switchKey(Keys key)
        {
            this.key = key;
        }
    }
    
}


