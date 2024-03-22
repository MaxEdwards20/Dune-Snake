using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
// Added to support serialization
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
        
         
namespace CS5410.IO
{
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
        