using Shared.Components;
using Shared.Entities;

namespace Shared.Systems;

public class CollisionHandler : Shared.Systems.System
{
    public override void update(TimeSpan elapsedTime)
    {
        throw new NotImplementedException();
    }
    
    public static void wormAteSpice(Entity worm, Entity spice)
    {
        // Remove the spice
        spice.remove<SpicePower>();
        // Grow the worm
        // TODO
    }
    
    public static void wormAteWorm(Entity worm1, Entity worm2)
    {
        // TODO
    }
    
    public static void wormHitWall(Entity worm, Entity wall)
    {
        // TODO
    }
}