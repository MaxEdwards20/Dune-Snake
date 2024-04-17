namespace Shared.Components;

public class ClientId : Component
{
    public int m_id { get; set; }
    
    public ClientId(int id)
    {
        m_id = id;
    }
}