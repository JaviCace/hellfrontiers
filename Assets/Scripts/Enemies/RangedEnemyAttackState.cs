using UnityEngine;
using player;

namespace Enemies
{

/// <summary>
/// Estado de ataque para enemigos a distancia.
/// </summary>
public class RangedEnemyAttackState : BaseEnemyAttackState
{
    /// <summary>
    /// segundos entre disparo y disparo de los enemigos a distancia
    /// </summary>
    private const float AttackInterval = 2f;

    public override void Enter(BaseEnemy enemy, object data = null)
    {
        // igual que BaseEnemyAttackState.Enter, pero con un intervalo fijo entre disparos
        enemy.isAttacking = true;
        if (!enemy.dead) enemy.PlayAttackAnimation();
        enemy.SafeDelay(AttackInterval, () =>
        {
            enemy.isAttacking = false;
        });

        ShootProjectile(enemy);

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
        if (enemy.ProjectilePrefabRef == null)
        {
            Debug.LogWarning($"[{enemy.name}] No tiene projectilePrefab asignado en el Inspector");
            return;
        }

        RangedEnemyProjectile projectile = Object.Instantiate(
            enemy.ProjectilePrefabRef,
            enemy.transform.position,
            Quaternion.identity
        );

        projectile.Init(enemy, Player.Instance);
    }
}
}
