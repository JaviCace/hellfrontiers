namespace Enemies
{

/// <summary>
/// Enemigo terrestre a distancia.
/// </summary>
public class GroundRangedEnemy : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();                                       // inicializar BaseEnemy

        // collider reducido
        colliderWidthDivider = 2f;                          // reducir ancho collider
        colliderHeightDivider = 1.2f;                       // reducir alto collider
        DivideCollider(colliderWidthDivider, colliderHeightDivider);

        // stats
        attackRange = 20f;                                  // rango de ataque
        attackDuration = 0.6f;                              // tiempo ataque
        canChase = false;                                   // no se mueve, solo dispara

        // estados
        stateMachine
            .AddState("move", new GroundEnemyMoveState())
            .AddState("attack", new RangedEnemyAttackState())
            .SetState("move");
    }
}
}
