using UnityEngine;

namespace Enemies
{

/// <summary>
/// Enemigo mina (explota al atacar).
/// </summary>
public class MineMeleeEnemy : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();                                       // inicializar BaseEnemy

        // stats
        health = 3f;
        transform.localScale = new Vector3(2f, 2f, 1f);     // escalar sprite
        attackRange = 1.5f;                                  // rango
        attackDuration = 1f;                                 // demora explosión
        damage = 1f;                                          // daño explosión
        meleeAttackWidge = 100f;                             // ancho hitbox
        meleeAttackHeight = 100f;                            // alto hitbox
        startAttackTime = 0f;                                // ataque inmediato

        // estados
        stateMachine
            .AddState("move", new GroundEnemyMoveState())
            .AddState("attack", new MineEnemyAttackState())
            .SetState("move");
    }

    /// <summary>
    /// sobreescribe colisión para que no haga doble daño (la explosión ya lo aplica)
    /// </summary>
    public override void CollisionWithPlayer()
    {
        // no hace nada
    }
}
}
