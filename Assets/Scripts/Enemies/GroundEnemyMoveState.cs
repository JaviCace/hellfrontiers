using UnityEngine;
using State;
using player;

namespace Enemies
{

/// <summary>
/// Estado de movimiento para enemigos terrestres.
/// </summary>
public class GroundEnemyMoveState : BaseState<BaseEnemy>
{
    private float startAttackTimer;

    public override void Enter(BaseEnemy enemy, object data = null)
    {
        startAttackTimer = 0f;                              // tiempo para cargar ataque
        enemy.PlayMoveAnimation();                          // anim caminar
    }

    public override void Execute(BaseEnemy enemy, float time, float delta)
    {
        Transform player = Player.Instance.transform;
        int direction = player.position.x > enemy.transform.position.x ? 1 : -1; // dirección hacia jugador
        float distance = enemy.DistanceToPlayer();                                // distancia real al jugador
        float heightDiff = Mathf.Abs(player.position.y - enemy.transform.position.y);

        enemy.SetVelocityX(0);

        // fuera del rango de detección: ni se mueve ni ataca
        if (!enemy.CanSeePlayer())
        {
            startAttackTimer = 0f;
            return;
        }

        // perseguir solo si puede moverse, aún no está en rango y el jugador está a su altura
        if (enemy.canChase && distance >= enemy.attackRange && heightDiff <= enemy.chaseHeightTolerance)
        {
            enemy.SetVelocityX(direction * enemy.speed);
            enemy.SetFlipX(direction < 0);
        }

        // cargar ataque cuando está en rango
        if (distance < enemy.attackRange)
        {
            startAttackTimer += delta;
        }
        else
        {
            startAttackTimer = 0f;
        }

        // lanzar ataque si ya cargó
        if (startAttackTimer > enemy.startAttackTime)
        {
            enemy.SetVelocityX(0);
            enemy.stateMachine.SetState("attack");
            startAttackTimer = 0f;
        }
    }

    public override void Exit(BaseEnemy enemy)
    {
        enemy.SetVelocityX(0);                              // detener al salir
    }
}
}
