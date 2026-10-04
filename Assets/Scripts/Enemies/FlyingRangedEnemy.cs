using UnityEngine;

namespace Enemies
{

/// <summary>
/// Enemigo volador con ataque a distancia.
/// </summary>
public class FlyingRangedEnemy : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();                                       // inicializar BaseEnemy

        // render
        transform.localScale = new Vector3(2f, 2f, 1f);     // más grande
        Rigidbody.gravityScale = 0f;                         // volador sin gravedad

        // collider reducido
        colliderWidthDivider = 2f;
        colliderHeightDivider = 2f;
        DivideCollider(colliderWidthDivider, colliderHeightDivider);

        // stats
        attackDuration = 1.5f;                                // duración ataque
        attackRange = 20f;                                    // rango ataque
        damage = 1f;                                          // daño

        // estados
        stateMachine
            .AddState("move", new FlyingEnemyMoveState())
            .AddState("attack", new RangedEnemyAttackState())
            .SetState("move");
    }
}
}
