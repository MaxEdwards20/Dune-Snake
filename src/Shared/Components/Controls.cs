using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
// Added to support serialization
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Shared.Components
{
    [DataContract(Name = "Controls")]
    public class Controls : Component
    {
        [DataMember(Name = "SnakeLeft")]
        public Control SnakeLeft = new Control(Keys.Left);
        [DataMember(Name = "SnakeRight")]
        public Control SnakeRight = new Control(Keys.Right);
        [DataMember (Name = "UseKeyboard")]
        public bool UseKeyboard = true;
    }
    
    [DataContract(Name = "Control")]
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


