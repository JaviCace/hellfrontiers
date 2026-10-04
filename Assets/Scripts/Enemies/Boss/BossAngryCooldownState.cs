namespace Enemies
{

/// <summary>
/// Estado de cooldown específico para el jefe Ira.
/// </summary>
public class BossAngryCooldownState : BaseCooldownState
{
    public BossAngryCooldownState() : base(new BossCooldownConfig
    {
        LogPrefix = "BossAngry",
        ResetVelocity = false
    })
    { }
}
}
