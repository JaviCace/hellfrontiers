using State;

namespace Enemies
{

/// <summary>
/// Estado base de ataque para enemigos.
/// </summary>
public class BaseEnemyAttackState : BaseState<BaseEnemy>
{
    public override void Enter(BaseEnemy enemy, object data = null)
    {
        enemy.isAttacking = true;                          // entrando en modo atacar
        if (!enemy.dead) enemy.PlayAttackAnimation();

        // terminar ataque tras duración
        enemy.SafeDelay(enemy.attackDuration, () =>
        {
            enemy.isAttacking = false;
        });
    }

    public override void Execute(BaseEnemy enemy, float time, float delta)
    {
        // si ya no está atacando, volver a estado mover
        if (!enemy.isAttacking)
        {
            enemy.stateMachine.SetState("move");
        }
    }
}
}
