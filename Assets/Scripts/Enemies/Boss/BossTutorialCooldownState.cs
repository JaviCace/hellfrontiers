namespace Enemies
{

/// <summary>
/// Estado de cooldown específico para el jefe Tutorial.
/// </summary>
public class BossTutorialCooldownState : BaseCooldownState
{
    public BossTutorialCooldownState() : base(new BossCooldownConfig
    {
        LogPrefix = "BossTutorial",
        ResetVelocity = true
    })
    { }
}
}
