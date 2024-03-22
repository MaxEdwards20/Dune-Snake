using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace CS5410.Objects { 
    public interface IParticleSystem { 
        Dictionary<long, Particle>.ValueCollection particles { get; }
        void update(GameTime gameTime);
    }

}