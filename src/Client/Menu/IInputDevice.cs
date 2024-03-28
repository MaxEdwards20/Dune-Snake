using Microsoft.Xna.Framework;

namespace Client.IO
{
    /// <summary>
    /// Abstract base class that defines how input is presented to game code.
    /// </summary>
    public interface IInputDevice
    {
        public delegate void CommandDelegate(GameTime gameTime, float value);
        public delegate void CommandDelegatePosition(GameTime GameTime, int x, int y);

        void Update(GameTime gameTime);
    }
}
