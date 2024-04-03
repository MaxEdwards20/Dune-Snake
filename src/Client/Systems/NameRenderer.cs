using System;
using Client.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
namespace Client.Systems;

public class NameRenderer : Shared.Systems.System 
{
    private GraphicsDeviceManager m_graphicsDeviceManager;
    private SpriteFont m_font;
    public NameRenderer(GraphicsDeviceManager graphicsDeviceManager, SpriteFont font) : base(
    typeof(Client.Components.Sprite), typeof(Name), typeof(Position)
    )
    {
       m_graphicsDeviceManager = graphicsDeviceManager;
       m_font = font;
    }
    
    public override void update(TimeSpan elapsedTime)
    {
        throw new NotImplementedException();
    }
    
    public void render(TimeSpan elapsedTime, SpriteBatch spriteBatch)
    {
        // spriteBatch.Begin();
        // // For every item that has a name, render it
        // foreach (var entity in m_entities)
        // {
        //     var position = entity.Value.get<Shared.Components.Position>().position;
        //     var name = entity.Value.get<Name>().name;
        //     Drawing.DrawShadedString(m_font, name, position, Color.White, spriteBatch);
        // }
        // spriteBatch.End();
    }
}