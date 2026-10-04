using State;

namespace Enemies
{

/// <summary>
/// Configuración de un estado de cooldown de jefe.
/// </summary>
public class BossCooldownConfig
{
    public string LogPrefix = "Boss";
    public bool ResetVelocity = false;
    public bool DisableMovement = false;
}

/// <summary>
/// Estado base de cooldown para todos los jefes.
/// </summary>
public class BaseCooldownState : BaseState<BaseBoss>
{
    protected BossCooldownConfig config;
    protected BaseBoss boss;
    protected float cooldownTime;

    public BaseCooldownState(BossCooldownConfig config = null)
    {
        this.config = config ?? new BossCooldownConfig();
    }

    public override void Enter(BaseBoss context, object data = null)
    {
        boss = context;
        cooldownTime = 0f;

        // resetear velocidad si está configurado
        if (config.ResetVelocity && boss.Rigidbody != null)
        {
            boss.Rigidbody.linearVelocity = UnityEngine.Vector2.zero;
        }

        // desactivar movimiento si está configurado (solo para bosses que no necesitan moverse)
        if (config.DisableMovement && boss.Rigidbody != null)
        {
            boss.Rigidbody.bodyType = UnityEngine.RigidbodyType2D.Kinematic;
        }
    }

    public override void Execute(BaseBoss context, float time, float delta)
    {
        cooldownTime += delta;

        if (cooldownTime >= boss.attackCooldown)
        {
            boss.StartRandomState();
        }
    }

    public override void Exit(BaseBoss context)
    {
        cooldownTime = 0f;

        if (config.DisableMovement && boss.Rigidbody != null)
        {
            boss.Rigidbody.bodyType = UnityEngine.RigidbodyType2D.Dynamic;
        }
    }
}
}
