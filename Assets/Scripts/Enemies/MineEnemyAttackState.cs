using UnityEngine;
using player;

namespace Enemies
{

/// <summary>
/// Estado de explosión para enemigo mina.
/// </summary>
public class MineEnemyAttackState : BaseEnemyAttackState
{
    private Vector2 hitboxCenter;
    private Vector2 hitboxSize;

    public override void Enter(BaseEnemy enemy, object data = null)
    {
        base.Enter(enemy, data);
        Explode(enemy);                                     // crear hitbox de explosión
    }

    public override void Execute(BaseEnemy enemy, float time, float delta)
    {
        base.Execute(enemy, time, delta);
    }

    /// <summary>
    /// crea el hitbox de explosión (solo guarda la zona; el daño se aplica en Exit)
    /// </summary>
    private void Explode(BaseEnemy enemy)
    {
        hitboxCenter = enemy.transform.position;
        hitboxSize = new Vector2(enemy.meleeAttackWidge, enemy.meleeAttackHeight);
    }

    public override void Exit(BaseEnemy enemy)
    {
        // aplicar daño si el jugador está dentro de la zona en el momento de salir del estado
        if (Player.Instance != null && Player.Instance.PlayerCollider != null)
        {
            Bounds hitboxBounds = new Bounds(hitboxCenter, hitboxSize);
            if (hitboxBounds.Intersects(Player.Instance.PlayerCollider.bounds))
            {
                int knockDir = Player.Instance.transform.position.x < enemy.transform.position.x ? -1 : 1;
                Player.Instance.TakeDamage(enemy.damage, knockDir);
            }
        }

        enemy.Die();                                        // la mina muere tras explotar
    }
}
}
