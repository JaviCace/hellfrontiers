namespace Enemies
{

/// <summary>
/// Estado de cooldown específico para el jefe Final.
/// </summary>
public class BossFinalCooldownState : BaseCooldownState
{
    public BossFinalCooldownState() : base(new BossCooldownConfig
    {
        LogPrefix = "BossFinal",
        ResetVelocity = false
    })
    { }
}
}
