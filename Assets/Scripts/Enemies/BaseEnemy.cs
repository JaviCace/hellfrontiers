using System;
using System.Collections;
using UnityEngine;
using State;
using player;

namespace Enemies
{

    /// <summary>
    /// Clase base de todos los enemigos.
    /// Contiene stats comunes, detección, daño y muerte.
    /// </summary>
    public class BaseEnemy : MonoBehaviour
    {
        // ---------------- Componentes (por Inspector, nunca GetComponent) ----------------
        [Header("Componentes")]
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected Animator animator;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected BoxCollider2D bodyCollider;     // collider físico principal (suelo, mundo)

        // ---------------- Animaciones ----------------
        [Header("Animaciones (nombres de estados del Animator)")]
        public string moveAnimationKey;
        public string attackAnimationKey;
        public string deathAnimationKey;

        // ---------------- Proyectil (solo enemigos a distancia) ----------------
        [Header("Proyectil (solo enemigos a distancia)")]
        [SerializeField] protected RangedEnemyProjectile projectilePrefab; // prefab con RangedEnemyProjectile ya puesto en la raíz
        public RangedEnemyProjectile ProjectilePrefabRef => projectilePrefab;

        // ---------------- Collider ----------------
        [HideInInspector] public float colliderWidthDivider = 1f;
        [HideInInspector] public float colliderHeightDivider = 1f;
        private Vector2 initialColliderSize;

        // ---------------- Stats ----------------
        [Header("Stats")]
        public float health = 6f;                                  // vida
        public float speed = 6f;                                   // velocidad movimiento
        public float verticalSpeed = 0f;                            // velocidad vertical
        public float attackRange = 80f;                             // rango ataque melee
        public float damage = 1f;                                   // daño base
        public float collisionDamage = 1f;                          // daño por colisión
        public float startAttackTime = 1f;                          // tiempo hasta atacar (segundos)
        public float attackDuration = 0.1f;                         // duración ataque (segundos)
        public float distanceBtwEnemies = 20f;                      // distancia mínima entre enemigos

        // ---------------- Estado y control ----------------
        public bool dead = false;                                   // enemigo está vivo
        public bool isAttacking = false;                            // está atacando
        public bool applyKnockbackToPlayer = true;                  // aplica knockback al jugador
        public bool inmune = false;                                 // ignora daño

        // ---------------- Detección del jugador ----------------
        [Header("Detección del jugador")]
        [Tooltip("Distancia (en unidades de mundo) a la que el enemigo se entera de que el jugador está cerca. Fuera de ella ni se mueve ni ataca")]
        public float detectionRange = 30f;
        [Tooltip("Si es false el enemigo nunca se mueve hacia el jugador (solo ataca cuando entra en su rango)")]
        public bool canChase = true;
        [Tooltip("Enemigos terrestres: solo persiguen si la diferencia de altura (Y) con el jugador es menor que esto")]
        public float chaseHeightTolerance = 2f;

        // ---------------- Física ----------------
        [Header("Física")]
        public float maxVelocityX = 1000f;
        public float maxVelocityY = 1000f;

        // ---------------- Hitbox melee (usado por los estados de ataque) ----------------
        [Header("Hitbox melee (usado por los estados de ataque)")]
        public float meleeAttackWidge;
        public float meleeAttackHeight;
        public float meleeAttackDist;

        public StateMachine<BaseEnemy> stateMachine;

        // acceso cómodo al jugador (singleton, ver Player.cs)
        protected Player player => Player.Instance;

        public Rigidbody2D Rigidbody => rb;
        public Animator EnemyAnimator => animator;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        protected virtual void Awake()
        {
            if (bodyCollider != null) initialColliderSize = bodyCollider.size;

            rb.gravityScale = 1f;                                   

            // máquina de estados (las subclases añaden aquí sus states concretos)
            stateMachine = new StateMachine<BaseEnemy>(this, "enemy");
        }

        /// <summary>
        /// update común de todos los enemigos
        /// </summary>
        protected virtual void Update()
        {
            if (dead || player == null || player.dead) return;

            stateMachine.Step(Time.time, Time.deltaTime);           // avanzar estado

            // comprobación de colisión con el jugador (equivalente al physics.add.overlap de Phaser)
            if (bodyCollider != null && player.PlayerCollider != null &&
                Physics2D.IsTouching(bodyCollider, player.PlayerCollider))
            {
                CollisionWithPlayer();
            }
        }

        /// <summary>
        /// comprueba si el jugador está en rango de detección
        /// </summary>
        public bool CanSeePlayer()
        {
            return DistanceToPlayer() < detectionRange;
        }

        /// <summary>
        /// distancia en línea recta (unidades de mundo) hasta el jugador
        /// </summary>
        public float DistanceToPlayer()
        {
            return Vector2.Distance(player.transform.position, transform.position);
        }

        /// <summary>
        /// reduce el tamaño del collider
        /// </summary>
        public void DivideCollider(float divW, float divH)
        {
            if (bodyCollider == null) return;

            Vector2 newSize = new Vector2(initialColliderSize.x / divW, initialColliderSize.y / divH);
            bodyCollider.size = newSize;
            bodyCollider.offset = new Vector2(
                (initialColliderSize.x - newSize.x) / 2f,
                (initialColliderSize.y - newSize.y) / 2f
            );
        }

        /// <summary>
        /// colisión entre jugador y enemigo
        /// </summary>
        public virtual void CollisionWithPlayer()
        {
            if (applyKnockbackToPlayer)
            {
                int dir = player.transform.position.x < transform.position.x ? 1 : -1; // dirección knockback
                player.TakeDamage(collisionDamage, dir);
            }
            else
            {
                player.TakeDamage(collisionDamage, null);
            }
        }

        /// <summary>
        /// recibir daño
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (inmune) return;

            StartCoroutine(DamageFeedbackRoutine());

            health -= amount;                                       // quitar vida

            if (health <= 0f) Die();                                // morir si llega a 0
        }

        /// <summary>
        /// tint rojo breve + parpadeo, igual que el tween de Phaser
        /// </summary>
        private IEnumerator DamageFeedbackRoutine()
        {
            if (spriteRenderer == null) yield break;

            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;

            // parpadeo (alpha 0 <-> 1, 3 veces)
            for (int i = 0; i < 3; i++)
            {
                yield return FadeTo(0f, 0.1f);
                yield return FadeTo(1f, 0.1f);
            }

            spriteRenderer.color = original;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float t = 0f;
            Color c = spriteRenderer.color;
            float startAlpha = c.a;
            while (t < duration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                spriteRenderer.color = c;
                yield return null;
            }
            c.a = targetAlpha;
            spriteRenderer.color = c;
        }

        /// <summary>
        /// muerte del enemigo
        /// </summary>
        public virtual void Die()
        {
            if (dead) return;

            dead = true;
            rb.gravityScale = 1f;                                   // gravedad al morir
            SetVelocityX(0);                                        // detener movimiento

            // robo de vida del jugador
            if (player != null && player.health < player.maxHealth)
            {
                player.health += Mathf.RoundToInt(player.bloodStealAmount);
                // TODO: notificar al HUD (equivalente a player.emit('updateHearts', health, false))
            }

            // animación de muerte
            if (!string.IsNullOrEmpty(deathAnimationKey))
            {
                PlayAnim(deathAnimationKey, true);
                StartCoroutine(DestroyAfterAnimation());
            }
            else
            {
                // muerte sin animación
                StartCoroutine(DestroyAfterDelay(0.1f));
            }
        }

        private IEnumerator DestroyAfterAnimation()
        {
            // espera a que el Animator termine el estado actual (la animación de muerte)
            yield return null; // deja que el Animator entre en el nuevo estado antes de medir
            float length = animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0.2f;
            yield return new WaitForSeconds(length);

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        public void PlayMoveAnimation()
        {
            if (!dead && !string.IsNullOrEmpty(moveAnimationKey))
                PlayAnim(moveAnimationKey, true);
        }

        public virtual void PlayAttackAnimation()
        {
            if (!dead && !string.IsNullOrEmpty(attackAnimationKey))
                PlayAnim(attackAnimationKey, true);
        }

        // ==================== Helpers (equivalentes a la API de Phaser) ====================

        public void SetVelocityX(float x) => rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
        public void SetVelocityY(float y) => rb.linearVelocity = new Vector2(rb.linearVelocity.x, y);
        public void SetVelocity(float x, float y) => rb.linearVelocity = new Vector2(x, y);

        public void SetFlipX(bool flip)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = flip;
        }

        /// <summary>
        /// equivalente a enemy.play(key, ignoreIfPlaying) de Phaser
        /// </summary>
        public void PlayAnim(string stateName, bool ignoreIfPlaying = false)
        {
            if (animator == null || string.IsNullOrEmpty(stateName)) return;
            if (ignoreIfPlaying && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) return;
            animator.Play(stateName);
        }

        /// <summary>
        /// equivalente a scene.time.delayedCall / safeDelay
        /// </summary>
        public Coroutine SafeDelay(float delaySeconds, Action callback)
        {
            return StartCoroutine(SafeDelayRoutine(delaySeconds, callback));
        }

        private IEnumerator SafeDelayRoutine(float delaySeconds, Action callback)
        {
            yield return new WaitForSeconds(delaySeconds);
            callback?.Invoke();
        }
    }
}