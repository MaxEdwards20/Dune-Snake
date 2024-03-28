using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Components.Appearance;

namespace Shared.Entities;

public class Particle
{
    public Entity create(  Vector2 position, Vector2 velocity, Vector2 size, TimeSpan lifeTime, float moveRate, float rotationRate )
    {
        Entity entity = new Entity();
        entity.add(new Position(position));
        entity.add(new Movement(moveRate, rotationRate));
        entity.add(new Appearance("Textures/particle")); // TODO
        entity.add(new Size(size));
        entity.add(new LifeTime(lifeTime));
        return entity;
    }
}