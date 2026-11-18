namespace Enemies
{

/// <summary>
/// Estado de cooldown específico para el jefe Miedo.
/// </summary>
public class BossFearCooldownState : BaseCooldownState
{
    public BossFearCooldownState() : base(new BossCooldownConfig
    {
        LogPrefix = "BossFear",
        ResetVelocity = false
    })
    { }
}
}
