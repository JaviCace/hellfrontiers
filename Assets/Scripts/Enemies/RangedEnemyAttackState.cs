using UnityEngine;
using player;

namespace Enemies
{

/// <summary>
/// Estado de ataque para enemigos a distancia.
/// </summary>
public class RangedEnemyAttackState : BaseEnemyAttackState
{
    private bool hasAttacked;

    public override void Enter(BaseEnemy enemy, object data = null)
    {
        base.Enter(enemy, data);
        hasAttacked = false;

        if (!hasAttacked)
        {
            ShootProjectile(enemy);
            hasAttacked = true;
        }

        enemy.Rigidbody.gravityScale = 0f;                  // no caer durante el ataque
    }

    public override void Execute(BaseEnemy enemy, float time, float delta)
    {
        base.Execute(enemy, time, delta);
    }

    /// <summary>
    /// dispara un proyectil hacia el jugador
    /// </summary>
    private void ShootProjectile(BaseEnemy enemy)
    {
        // el prefab del proyectil se asigna por Inspector en cada enemigo (campo projectilePrefab de BaseEnemy).
        // Instanciamos directamente el componente tipado, sin GetComponent.
        RangedEnemyProjectile projectile = Object.Instantiate(
            enemy.ProjectilePrefabRef,
            enemy.transform.position,
            Quaternion.identity
        );

        projectile.Init(enemy, Player.Instance);
    }
}
}
