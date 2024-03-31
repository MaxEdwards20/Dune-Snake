using Shared.Entities;
using System;
using System.Diagnostics;

namespace Client.Systems;

public class Camera : Shared.Systems.System
{
    public Camera() :
        base(
            typeof(Shared.Components.Position),
            typeof(Shared.Components.Movement),
            typeof(Shared.Components.Input)
        )
    { }

    public override void update(TimeSpan elapsedTime)
    {
        
        foreach (Entity entity in m_entities.Values)
            Debug.WriteLine(entity);
    }
}