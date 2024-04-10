using Shared.Components;
using Shared.Entities;

namespace Shared.Systems;

public class CollisionHandler : Shared.Systems.System
{
    public override void update(TimeSpan elapsedTime)
    {
        // Basically we look at each worm head and see how big it is. If it is above a certain threshold, then we update its size and remove the spice power.
    }
    
}