namespace Shared.Components;

public class SpicePower: Component
{
    public int power { get; private set; }
    public SpicePower(int power)
    {
        this.power = power;
    }
    
    public void addPower(int power)
    {
        this.power += power;
    }
    
    public void removePower(int power)
    {
        this.power -= power;
    }
    
    public void setPower(int power)
    {
        this.power = power;
    }
    
    public void resetPower()
    {
        this.power = 0;
    }
}