namespace Enemies
{

/// <summary>
/// Enemigo terrestre de ataque cuerpo a cuerpo.
/// </summary>
public class GroundMeleeEnemy : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();                                       // inicializar BaseEnemy

        // stats
        attackRange = 1.5f;                                 // rango ataque
        attackDuration = 1f;
        meleeAttackWidge = 60f;                             // ancho hitbox
        meleeAttackHeight = 60f;                            // alto hitbox
        meleeAttackDist = 40f;                              // distancia ataque

        // estados
        stateMachine
            .AddState("move", new GroundEnemyMoveState())
            .AddState("attack", new MeleeEnemyAttackState())
            .SetState("move");
    }

    /// <summary>
    /// vacío porque su animación de ataque es un sprite aparte (ver MeleeEnemyAttackState)
    /// </summary>
    public override void PlayAttackAnimation()
    {
    }
}
}
