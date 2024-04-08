using Microsoft.Xna.Framework;

namespace Shared.Components;

public class AnchorQueue: Component
{
    public Queue<Position> m_anchorPositions = new Queue<Position>();
    
}