using System.Collections;
using UnityEngine;
using player;

namespace Enemies
{

/// <summary>
/// Estado de ataque cuerpo a cuerpo con animación (squash/stretch + hitbox).
/// </summary>
public class MeleeEnemyAttackState : BaseEnemyAttackState
{
    private bool hasAttacked;
    private GameObject meleeVisual;

    public override void Enter(BaseEnemy enemy, object data = null)
    {
        base.Enter(enemy, data);

        hasAttacked = false;                                // evita doble ataque
        meleeVisual = null;

        int direction = player_x_greater(enemy) ? 1 : -1;

        if (!hasAttacked)
        {
            enemy.StartCoroutine(AttackScaleTween(enemy, direction));
        }
    }

    private bool player_x_greater(BaseEnemy enemy)
    {
        return Player.Instance.transform.position.x > enemy.transform.position.x;
    }

    public override void Execute(BaseEnemy enemy, float time, float delta)
    {
        base.Execute(enemy, time, delta);                   // termina ataque si ya acabó
    }

    /// <summary>
    /// equivalente al tween scaleX/scaleY con yoyo de Phaser: achata al enemigo y,
    /// en el punto máximo (el "yoyo"), suelta el golpe; luego vuelve a su forma original.
    /// </summary>
    private IEnumerator AttackScaleTween(BaseEnemy enemy, int direction)
    {
        Vector3 baseScale = enemy.transform.localScale;
        Vector3 attackScale = new Vector3(baseScale.x * 1.3f, baseScale.y * 0.7f, baseScale.z);
        float half = enemy.attackDuration * 0.8f / 2f;

        // ida (forma achatada)
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            enemy.transform.localScale = Vector3.Lerp(baseScale, attackScale, t / half);
            yield return null;
        }
        enemy.transform.localScale = attackScale;

        // momento del yoyo: soltar el golpe
        if (enemy.gameObject.activeInHierarchy)
        {
            MeleeAttack(enemy, direction);
            hasAttacked = true;
        }

        // vuelta (forma original)
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            enemy.transform.localScale = Vector3.Lerp(attackScale, baseScale, t / half);
            yield return null;
        }
        enemy.transform.localScale = baseScale;

        // limpiar tinte por seguridad al terminar todo
        if (enemy.SpriteRenderer != null) enemy.SpriteRenderer.color = Color.white;
    }

    /// <summary>
    /// realiza ataque cuerpo a cuerpo con hitbox y animación
    /// </summary>
    /// <param name="direction">dirección del ataque (1: derecha, -1: izquierda)</param>
    private void MeleeAttack(BaseEnemy enemy, int direction)
    {
        float w = enemy.meleeAttackWidge;                    // ancho hitbox
        float h = enemy.meleeAttackHeight;                   // alto hitbox
        float offsetX = direction * enemy.meleeAttackDist;
        Vector2 center = (Vector2)enemy.transform.position + new Vector2(offsetX, 0f);

        // sprite de animación de ataque (visual), tal y como en el original
        meleeVisual = new GameObject("MeleeAttackVisual");
        meleeVisual.transform.position = center;
        var renderer = meleeVisual.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = enemy.SpriteRenderer != null ? enemy.SpriteRenderer.sortingOrder + 1 : 1;
        renderer.flipX = direction == -1;

        var visualAnimator = meleeVisual.AddComponent<Animator>();
        // TODO: asigna aquí el Animator Controller correspondiente a enemy.attackAnimationKey
        // (en Phaser se resolvía dinámicamente por clave; en Unity necesitas un controller de referencia,
        // por ejemplo exponiendo un [SerializeField] Animator/AnimatorController en el enemigo)

        // NOTA: en Phaser el rectángulo del hitbox se crea con (x, y, h, w, ...), es decir,
        // ancho=h y alto=w -- los ejes vienen intercambiados en el original. Lo mantengo igual
        // para que el comportamiento sea idéntico; revisa si esto era intencional en tu juego.
        bool damaged = false;

        enemy.StartCoroutine(MonitorHitbox(enemy, center, new Vector2(h, w), () =>
        {
            if (damaged) return;
            damaged = true;

            int knockDir = Player.Instance.transform.position.x < enemy.transform.position.x ? -1 : 1;
            Player.Instance.TakeDamage(enemy.damage, knockDir);
        }));

        // destruir sprite + hitbox cuando termine la animación (aproximado con attackDuration)
        enemy.SafeDelay(enemy.attackDuration, () =>
        {
            if (meleeVisual != null) Object.Destroy(meleeVisual);
        });
    }

    /// <summary>
    /// comprueba cada frame si el jugador entra en la zona del hitbox mientras dure el ataque
    /// (equivalente al physics.add.overlap persistente de Phaser)
    /// </summary>
    private IEnumerator MonitorHitbox(BaseEnemy enemy, Vector2 center, Vector2 size, System.Action onHit)
    {
        float elapsed = 0f;
        while (elapsed < enemy.attackDuration)
        {
            elapsed += Time.deltaTime;

            if (Player.Instance != null && Player.Instance.PlayerCollider != null)
            {
                Bounds hitboxBounds = new Bounds(center, size);
                if (hitboxBounds.Intersects(Player.Instance.PlayerCollider.bounds))
                {
                    onHit();
                    yield break;
                }
            }
            yield return null;
        }
    }
}
}
