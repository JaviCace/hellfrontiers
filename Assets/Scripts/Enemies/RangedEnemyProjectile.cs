using UnityEngine;
using player;

namespace Enemies
{

/// <summary>
/// Proyectil disparado por un enemigo a distancia.
/// </summary>
public class RangedEnemyProjectile : MonoBehaviour
{
    [Header("Componentes (por Inspector)")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D projectileCollider;   // trigger, para detectar al jugador

    [Tooltip("Velocidad en unidades de mundo por segundo (los 250 de Phaser eran píxeles)")]
    public float projectileSpeed = 4f;
    public float lifetime = 5f;                                 // vida máxima del proyectil (segundos)
    [Tooltip("Se destruye si se aleja tanto del jugador (ya no se destruye al salir de la cámara)")]
    public float maxDistanceFromPlayer = 40f;

    private BaseEnemy enemy;                                    // enemigo que dispara
    private Transform target;                                   // objetivo (jugador)
    private float elapsed;

    /// <summary>
    /// equivalente al constructor de Phaser. Llamar justo después de Instantiate.
    /// </summary>
    public void Init(BaseEnemy sourceEnemy, Player targetPlayer)
    {
        enemy = sourceEnemy;
        target = targetPlayer.transform;

        rb.gravityScale = 0f;                                   // ignorar gravedad

        // vector hacia el jugador
        Vector2 dir = (Vector2)(target.position - transform.position);
        dir.Normalize();

        rb.linearVelocity = dir * projectileSpeed;

        // rotación según movimiento
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        elapsed = 0f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (target == null || !target.gameObject.activeInHierarchy || elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // destruir si se aleja demasiado del jugador. Antes se destruía al salir de la cámara, pero con
        // rango de ataque 20 el enemigo dispara desde fuera de pantalla y el proyectil moría al nacer
        if (Vector2.Distance(transform.position, target.position) > maxDistanceFromPlayer)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// colisión con el jugador (projectileCollider debe ser un trigger)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        int knockDir = target.position.x < enemy.transform.position.x ? -1 : 1;
        Player.Instance.TakeDamage(enemy.damage, knockDir);
        Destroy(gameObject);
    }
}
}
