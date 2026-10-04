namespace Enemies
{

/// <summary>
/// Estado de cooldown específico para el jefe Tristeza.
/// </summary>
public class BossSadCooldownState : BaseCooldownState
{
    public BossSadCooldownState() : base(new BossCooldownConfig
    {
        LogPrefix = "BossSad",
        ResetVelocity = false
    })
    { }
}
}
