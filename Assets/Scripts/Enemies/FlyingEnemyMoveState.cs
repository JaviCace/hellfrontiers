using UnityEngine;
using State;
using player;

namespace Enemies
{

/// <summary>
/// Estado de movimiento para enemigos voladores.
/// </summary>
public class FlyingEnemyMoveState : BaseState<BaseEnemy>
{
    private float startAttackTimer;

    public override void Enter(BaseEnemy enemy, object data = null)
    {
        startAttackTimer = 0f;                              // tiempo acumulado para atacar
        enemy.PlayMoveAnimation();                          // anim caminar
    }

    public override void Execute(BaseEnemy enemy, float time, float delta)
    {
        Transform player = Player.Instance.transform;

        float dx = player.position.x - enemy.transform.position.x; // distancia x
        float dy = player.position.y - enemy.transform.position.y; // distancia y
        float distance = Mathf.Sqrt(dx * dx + dy * dy);            // distancia total

        if (enemy.CanSeePlayer())
        {
            // entrar en ataque si está dentro del rango
            if (distance < enemy.attackRange)
            {
                startAttackTimer += delta;
                enemy.stateMachine.SetState("attack");
            }
            else
            {
                startAttackTimer = 0f;
            }

            // perseguir jugador mientras no esté en rango
            if (distance > enemy.attackRange)
            {
                float nx = dx / distance;                    // normalizar x
                float ny = dy / distance;                     // normalizar y

                enemy.SetVelocity(
                    nx * enemy.speed,                          // velocidad horizontal
                    ny * enemy.verticalSpeed                   // velocidad vertical
                );

                enemy.SetFlipX(nx < 0);                        // voltear sprite
            }

            // si cargó el ataque suficiente, atacar
            if (startAttackTimer > enemy.startAttackTime)
            {
                enemy.SetVelocityX(0);
                enemy.stateMachine.SetState("attack");
                startAttackTimer = 0f;
            }
        }
        else
        {
            enemy.SetVelocity(0, 0);                          // si no ve al jugador, detenerse
        }
    }

    public override void Exit(BaseEnemy enemy)
    {
        enemy.SetVelocityX(0);                                // detener al salir del estado
        enemy.SetVelocityY(0);
    }
}
}
