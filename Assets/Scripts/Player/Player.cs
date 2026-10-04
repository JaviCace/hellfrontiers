using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using State;
using Enemies;

namespace player
{

    /// <summary>
    /// Clase player.
    /// Representa al jugador controlado por el usuario.
    /// </summary>
    public class Player : MonoBehaviour
    {
        // Singleton: permite que los enemigos encuentren al jugador sin arrastrarlo
        // a mano en cada prefab 
        public static Player Instance { get; private set; }


        [Header("Componentes")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D playerCollider;       // collider principal del jugador, usado por los enemigos para detectar overlap
        public Collider2D PlayerCollider => playerCollider;

        [Header("Ground check")]
        [SerializeField] private LayerMask groundLayer;

        // ---------------- Vida ----------------
        [Header("Vida")]
        public int health = 5;                                    // vida actual
        public int maxHealth = 5;                                 // vida máxima
        public bool dead = false;                                 // estado muerto

        // ---------------- Ataque ----------------
        [Header("Ataque")]
        public int damage = 3;                                    // daño melee
        public float meleeAttackDist = 100f;                       // distancia hitbox
        public float meleeAttackWidge = 120f;                      // ancho hitbox
        public float meleeAttackHeight = 70f;                      // alto hitbox
        public float attackCooldown = 0.75f;                       // cooldown melee (segundos)
        public float attackDuration = 0.2f;                        // duración hitbox (segundos)
        public bool isAttacking = false;                           // está atacando
        [SerializeField] private LayerMask enemyLayer;             // layer de los enemigos para el overlap
        [SerializeField] private GameObject meleeSpritePrefab;     // sprite normal de la animación de ataque
        [SerializeField] private GameObject meleeAmpliadoSpritePrefab; // sprite ampliado de la animación de ataque

        // ---------------- Movimiento ----------------
        [Header("Movimiento")]
        public int direction = 1;                                  // dirección horizontal
        public float movementSpeed = 300f;                         // velocidad movimiento
        public bool canMove = true;                                // puede moverse

        // ---------------- Salto ----------------
        [Header("Salto")]
        public float jumpSpeed = 800f;                             // velocidad salto
        public float jumpSpeedModifier = 1f;                       // modificador salto
        public bool canPogoJump = false;                           // pogo jump activo
        public float pogoJumpJudgeTime = 0.1f;                     // ventana pogo (segundos)
        public float pogoJumpSpeed = 600f;                          // fuerza pogo
        public float jumpBufferTime = 0.2f;                        // tiempo buffer salto (segundos)
        public float jumpBufferTimer = 0f;                          // timer buffer salto

        // ---------------- Dash ----------------
        [Header("Dash")]
        public bool canDash = false;                                // tiene dash
        public bool isDashing = false;                              // estado dash
        public float dashSpeed = 800f;                              // velocidad dash
        public float dashDuration = 0.2f;                           // duración dash (segundos)
        public float dashCooldown = 0.5f;                           // cooldown dash (segundos)
        public float dashCooldownTimer = 0f;                        // timer cooldown dash
        [SerializeField] private GameObject dashGhostPrefab;        // prefab del sprite fantasma del dash

        // ---------------- Escudo ----------------
        [Header("Escudo")]
        public bool canShield = false;                               // tiene escudo
        public bool hasShield = false;                               // escudo activo
        public float shieldCooldown = 5f;                            // cooldown escudo (segundos)
        public float shieldCooldownTimer = 0f;                       // timer cooldown escudo
        [SerializeField] private GameObject shieldAura;              // aura visual del escudo

        // ---------------- Invulnerabilidad / knockback ----------------
        [Header("Invulnerabilidad")]
        public bool invulnerable = false;                            // estado invulnerable
        public float initialInvulnerableTime = 1f;                   // invulnerabilidad inicial (segundos)
        public float invulnerableTime = 1f;                          // duración invulnerable (segundos)
        public float knockbackTime = 0.2f;                           // tiempo knockback (segundos)
        public float knockbackDistance = 200f;                       // distancia knockback

        // ---------------- Orbes ----------------
        [Header("Orbes")]
        public List<Orbs.BaseOrb> orbs = new List<Orbs.BaseOrb>();  // lista orbes recogidos
        public Orbs.BaseOrb[] equippedOrbs = new Orbs.BaseOrb[2];   // orbes equipados
        public int activeOrbIndex = 0;                               // orbe activo
        public float damageMultiplier = 1f;                          // multiplicador daño
        public float speedMultiplier = 1f;                           // multiplicador velocidad
        public float bloodStealAmount = 0f;                          // robo de vida
        public float attackRangeMultiplier = 1f;                     // modificador rango melee
        public Color orbTint = Color.white;                          // color original

        // ---------------- Referencias externas ----------------
        [Header("Referencias externas")]
        [SerializeField] private CameraFollow cameraFollow;          // referencia a la cámara que sigue al jugador
        [SerializeField] private string gameOverSceneName = "GameOver";
        [SerializeField] private Sprite deadSprite;                  // textura de "defeat_player"

        // ---------------- Input (un botón por acción, ver métodos OnXxx más abajo) ----------------
        [HideInInspector] public bool moveLeftHeld = false;
        [HideInInspector] public bool moveRightHeld = false;
        private bool jumpPressed = false;      // "JustDown" del botón salto
        private bool useOrbPressed = false;    // "JustDown" del botón usar orbe
        private bool attackUpPressed = false;
        private bool attackDownPressed = false;
        private bool attackLeftPressed = false;
        private bool attackRightPressed = false;

        // ---------------- Estado interno ----------------
        private string attackDir = null;
        private Coroutine dashEndCoroutine;
        private Coroutine ghostTimerCoroutine;

        // ---------------- Máquina de estados ----------------
        public StateMachine<Player> stateMachine;

        public Rigidbody2D Rigidbody => rb;
        public Animator PlayerAnimator => animator;

        /// <summary>
        /// equivalente al constructor de Phaser (lo que no depende de instanciar el GameObject va en Awake)
        /// </summary>
        private void Awake()
        {
            Instance = this;

            rb.gravityScale = 1f;                                   // gravedad (ajusta a tu Project Settings)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            invulnerableTime = initialInvulnerableTime;

            // estados
            stateMachine = new StateMachine<Player>(this, "player");
            stateMachine
                .AddState("idle", new PlayerIdleState())
                .AddState("move", new PlayerMoveState())
                .AddState("jump", new PlayerJumpState())
                .AddState("knockback", new PlayerKnockbackState())
                .AddState("dead", new PlayerDeathState())
                .AddState("dash", new PlayerDashState());
        }

        private void Start()
        {
            stateMachine.SetState("idle");

            if (shieldAura != null) shieldAura.SetActive(false);
        }

        /// <summary>
        /// update principal del jugador.
        /// controla estado, ataques, timers y efectos.
        /// (el input de movimiento ya llega como bools desde los botones de la UI)
        /// </summary>
        private void Update()
        {
            if (dead) return;

            float time = Time.time;
            float delta = Time.deltaTime;

            // si un efecto impide movimiento, cancelar input
            if (!canMove)
            {
                SetVelocityX(0);
                return;
            }

            // buffer de salto, permite saltar unos ms después de pulsar el botón
            // (esto va ANTES del stateMachine.Step para que el salto de este mismo frame ya lo vea)
            if (jumpPressed)
            {
                jumpBufferTimer = jumpBufferTime;
            }
            else
            {
                jumpBufferTimer -= delta;
            }
            jumpPressed = false; // consumido

            // avanzar máquina de estados
            stateMachine.Step(time, delta);

            // detectar dirección de ataque cuerpo a cuerpo
            attackDir = GetAttackDirection();

            // manejo de orbes (botón "usar orbe")
            if (useOrbPressed)
            {
                // prioridad 1: dash
                if (canDash && dashCooldownTimer <= 0f && !isDashing)
                {
                    stateMachine.SetState("dash");
                }
                // prioridad 2: escudo
                else if (canShield && !hasShield && shieldCooldownTimer <= 0f)
                {
                    if (shieldAura != null) shieldAura.SetActive(true);
                    hasShield = true;
                }
            }
            useOrbPressed = false; // consumido, equivalente a JustDown de un solo frame

            // ataque melee
            if (attackDir != null && !isDashing)
            {
                PerformMeleeAttack(attackDir);
            }

            // reducir timers (enfriamientos)
            dashCooldownTimer -= delta;
            shieldCooldownTimer -= delta;

            // seguir el jugador con el aura del escudo
            if (shieldAura != null)
            {
                shieldAura.transform.position = transform.position;
            }
        }

        // ==================== Input: un método por botón (engánchalos desde EventTrigger / Button en la UI) ====================

        public void OnMoveLeftDown() => moveLeftHeld = true;
        public void OnMoveLeftUp() => moveLeftHeld = false;
        public void OnMoveRightDown() => moveRightHeld = true;
        public void OnMoveRightUp() => moveRightHeld = false;
        public void OnJumpButtonDown() => jumpPressed = true;
        public void OnUseOrbButtonDown() => useOrbPressed = true;
        public void OnChangeOrbButtonDown() => SwitchActiveOrb();
        public void OnAttackUpButtonDown() => attackUpPressed = true;
        public void OnAttackDownButtonDown() => attackDownPressed = true;
        public void OnAttackLeftButtonDown() => attackLeftPressed = true;
        public void OnAttackRightButtonDown() => attackRightPressed = true;

        /// <summary>
        /// dirección de ataque, consumiendo el botón pulsado ese frame (equivalente a JustDown)
        /// </summary>
        private string GetAttackDirection()
        {
            if (attackUpPressed) { attackUpPressed = false; return "up"; }
            if (attackDownPressed) { attackDownPressed = false; return "down"; }
            if (attackLeftPressed) { attackLeftPressed = false; return "left"; }
            if (attackRightPressed) { attackRightPressed = false; return "right"; }
            return null;
        }

        // ==================== Helpers de física / animación (equivalentes a la API de Phaser) ====================

        public void SetVelocityX(float x) => rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
        public void SetVelocityY(float y) => rb.linearVelocity = new Vector2(rb.linearVelocity.x, y);
        public void SetVelocity(float x, float y) => rb.linearVelocity = new Vector2(x, y);
        public float VelocityX => rb.linearVelocity.x;

        public float GravityScale
        {
            get => rb.gravityScale;
            set => rb.gravityScale = value;
        }

        public void SetFlipX(bool flip)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = flip;
        }

        /// <summary>
        /// equivalente a player.play(key, ignoreIfPlaying) de Phaser
        /// </summary>
        public void PlayAnim(string stateName, bool ignoreIfPlaying = false)
        {
            if (animator == null) return;
            if (ignoreIfPlaying && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) return;
            animator.Play(stateName);
        }

        public bool IsGrounded()
        {
            return playerCollider != null && playerCollider.IsTouchingLayers(groundLayer);
        }

        /// <summary>
        /// equivalente a scene.time.delayedCall + safeDelay del original
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

        /// <summary>
        /// equivalente a scene.time.addEvent({ delay, loop: true, callback })
        /// </summary>
        public Coroutine StartRepeatingTimer(float delaySeconds, Action callback)
        {
            return StartCoroutine(RepeatingRoutine(delaySeconds, callback));
        }

        private IEnumerator RepeatingRoutine(float delaySeconds, Action callback)
        {
            var wait = new WaitForSeconds(delaySeconds);
            while (true)
            {
                yield return wait;
                callback?.Invoke();
            }
        }

        public void StopRepeatingTimer(Coroutine routine)
        {
            if (routine != null) StopCoroutine(routine);
        }

        // ==================== Colisión con los límites del mundo (equivalente a 'worldbounds' de Arcade Physics) ====================
        // Requiere que los colliders de los límites del nivel tengan el tag "WorldBound".
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDashing && collision.gameObject.CompareTag("WorldBound"))
            {
                isDashing = false;
            }
        }

        // ==================== Daño / muerte ====================

        /// <summary>
        /// aplica daño al jugador
        /// </summary>
        public void TakeDamage(float dmg, int? knockbackDirection)
        {
            // si está invulnerable, ignorar daño
            if (invulnerable) return;

            // tint rojo breve para feedback visual
            if (spriteRenderer != null) spriteRenderer.color = Color.red;

            // quitar tinte un poco antes de que acabe la invulnerabilidad
            SafeDelay(invulnerableTime * 0.8f, () =>
            {
                if (spriteRenderer != null) spriteRenderer.color = orbTint;
            });

            // activar invulnerabilidad temporal
            invulnerable = true;
            SafeDelay(invulnerableTime, () => invulnerable = false);

            // escudo bloqueador: solo bloquea daño leve (1)
            if (hasShield && Mathf.Approximately(dmg, 1f))
            {
                shieldCooldownTimer = shieldCooldown;          // poner cooldown
                hasShield = false;                             // quitar escudo
                if (shieldAura != null) shieldAura.SetActive(false); // ocultar efecto

                return;
            }

            // aplicar daño real
            health -= Mathf.RoundToInt(dmg);

            // TODO: notificar al HUD (equivalente a this.emit('updateHearts', health, true))

            // si muere, ejecutar muerte
            if (health <= 0)
            {
                Die();
            }

            // si no ha muerto, aplicar knockback
            if (knockbackDirection.HasValue && !dead)
            {
                stateMachine.SetState("knockback", knockbackDirection.Value);
            }
        }

        // ==================== Orbes ====================

        /// <summary>
        /// el jugador recoge un orbe del suelo
        /// </summary>
        public void CollectOrb(Orbs.BaseOrb orb)
        {
            // si nunca lo tenía, agregarlo a la lista
            if (!orbs.Contains(orb))
            {
                orbs.Add(orb);
            }

            // intentar autoequiparlo en el primer hueco libre
            for (int i = 0; i < equippedOrbs.Length; i++)
            {
                if (equippedOrbs[i] == null)
                {
                    EquipOrb(i, orb);

                    // si es el primer orbe recogido, activarlo automáticamente
                    if (activeOrbIndex == i)
                    {
                        ActivateOrb(i);
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// equipa un orbe en un slot concreto
        /// </summary>
        public void EquipOrb(int slotIndex, Orbs.BaseOrb orb)
        {
            if (orb == null) return;
            if (slotIndex < 0 || slotIndex > 1) return;
            if (!orbs.Contains(orb)) return;

            // si había un orbe equipado, desactivar su efecto
            if (equippedOrbs[slotIndex] != null)
            {
                equippedOrbs[slotIndex].OnDesactivate(this);
            }

            // equipar nuevo orbe (tu BaseOrb no tiene OnEquip, solo OnActivate/OnDesactivate)
            equippedOrbs[slotIndex] = orb;

            // TODO: notificar a la UI (equivalente a this.emit('orbChanged'))
        }

        /// <summary>
        /// desequipar orbe
        /// </summary>
        public void DesEquipOrb(int slotIndex)
        {
            if (equippedOrbs[slotIndex] == null) return;

            equippedOrbs[slotIndex].OnDesactivate(this);
            equippedOrbs[slotIndex] = null;

            SwitchActiveOrb();
            // TODO: notificar a la UI (equivalente a this.emit('orbChanged'))
        }

        /// <summary>
        /// cambiar orbe activo
        /// </summary>
        public void SwitchActiveOrb()
        {
            int nextIndex = (activeOrbIndex + 1) % equippedOrbs.Length;
            ActivateOrb(nextIndex);
        }

        /// <summary>
        /// activa el orbe del slot especificado
        /// </summary>
        public void ActivateOrb(int slotIndex)
        {
            Orbs.BaseOrb currentOrb = equippedOrbs[activeOrbIndex];
            Orbs.BaseOrb nextOrb = equippedOrbs[slotIndex];

            // si no hay orbe en ese slot, cancelar
            if (nextOrb == null) return;

            // desactivar orbe actual (si hay)
            currentOrb?.OnDesactivate(this);

            // activar nuevo orbe
            nextOrb.OnActivate(this);

            // guardar índice activo
            activeOrbIndex = slotIndex;

            // TODO: notificar a la UI (equivalente a this.emit('orbChanged'))
        }

        /// <summary>
        /// muerte del jugador
        /// </summary>
        public void Die()
        {
            if (dead) return;

            if (shieldAura != null) shieldAura.SetActive(false);

            health = maxHealth;
            dead = true;
            canMove = false;
            isAttacking = false;
            isDashing = false;

            if (animator != null) animator.enabled = false;
            if (spriteRenderer != null && deadSprite != null) spriteRenderer.sprite = deadSprite;

            if (cameraFollow != null) cameraFollow.StopFollow();

            StartCoroutine(DeathTimelineRoutine());
        }

        /// <summary>
        /// equivalente al timeline de tweens de la muerte (sube y luego cae fuera de cámara)
        /// </summary>
        private IEnumerator DeathTimelineRoutine()
        {
            Vector3 start = transform.position;
            Vector3 up = start + Vector3.up * 1.2f;      // "-=120" en px de Phaser -> ajusta la unidad a tu escena
            Vector3 down = up + Vector3.down * 5f;        // "+=500" en px de Phaser -> ajusta la unidad a tu escena

            yield return MoveOverTime(start, up, 0.7f);
            yield return MoveOverTime(up, down, 0.9f);

            yield return new WaitForSeconds(1.5f);

            if (!string.IsNullOrEmpty(gameOverSceneName))
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                SceneManager.LoadScene(gameOverSceneName, LoadSceneMode.Additive);
            }

            SetVelocity(0, 0);
            stateMachine.SetState("dead");
        }

        private IEnumerator MoveOverTime(Vector3 from, Vector3 to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, t / duration);
                yield return null;
            }
            transform.position = to;
        }

        // ==================== Ataques ====================

        /// <summary>
        /// ataque cuerpo a cuerpo
        /// </summary>
        public void PerformMeleeAttack(string dir)
        {
            if (isAttacking) return;

            isAttacking = true;

            SafeDelay(attackCooldown, () => isAttacking = false);

            float offsetX = 0f, offsetY = 0f;
            float w = meleeAttackWidge;
            float h = meleeAttackHeight * attackRangeMultiplier;

            switch (dir)
            {
                case "left": offsetX = -meleeAttackDist; break;
                case "right": offsetX = meleeAttackDist; break;
                case "up": offsetY = meleeAttackDist; (w, h) = (h, w); break;
                case "down": offsetY = -meleeAttackDist; (w, h) = (h, w); break;
            }

            Vector2 hitboxCenter = (Vector2)transform.position + new Vector2(offsetX, offsetY);

            // detectar colisiones con enemigos (única excepción a "nunca GetComponent":
            // los enemigos son objetos dinámicos, no se pueden fijar por Inspector)
            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, new Vector2(h, w), 0f, enemyLayer);
            HashSet<BaseEnemy> hitEnemies = new HashSet<BaseEnemy>();
            HashSet<BaseBoss> hitBosses = new HashSet<BaseBoss>();

            // rectángulo rojo de depuración, del mismo tamaño y posición que el hitbox real
            GameObject debugRect = BaseBossAttackState.CreateColorSpriteObject("AttackHitboxDebug", h, w, Color.red, 0.5f);
            debugRect.transform.position = hitboxCenter;
            SafeDelay(attackDuration, () => { if (debugRect != null) Destroy(debugRect); });

            foreach (var hit in hits)
            {
                // jefes: BaseBoss no hereda de BaseEnemy, así que se comprueban aparte
                // (GetComponentInParent por si el collider está en un hijo del jefe)
                var boss = hit.GetComponentInParent<BaseBoss>();
                if (boss != null)
                {
                    if (hitBosses.Add(boss))
                    {
                        boss.TakeDamage(damage * damageMultiplier);

                        if (dir == "down")
                        {
                            canPogoJump = true;
                            SafeDelay(pogoJumpJudgeTime, () => canPogoJump = false);
                        }
                    }
                    continue;
                }

                if (!hit.TryGetComponent<BaseEnemy>(out var enemy)) continue;
                if (hitEnemies.Contains(enemy)) continue;
                hitEnemies.Add(enemy);

                enemy.TakeDamage(damage * damageMultiplier);

                if (dir == "down")
                {
                    canPogoJump = true;
                    SafeDelay(pogoJumpJudgeTime, () => canPogoJump = false);
                }
            }

            // animación de ataque
            GameObject prefabToSpawn = !Mathf.Approximately(attackRangeMultiplier, 1f) ? meleeAmpliadoSpritePrefab : meleeSpritePrefab;
            GameObject meleeInstance = null;
            if (prefabToSpawn != null)
            {
                meleeInstance = Instantiate(prefabToSpawn, hitboxCenter, Quaternion.identity);

                var meleeRenderer = meleeInstance.GetComponentInChildren<SpriteRenderer>();
                switch (dir)
                {
                    case "right":
                        if (meleeRenderer != null) meleeRenderer.flipX = false;
                        meleeInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case "left":
                        if (meleeRenderer != null) meleeRenderer.flipX = true;
                        meleeInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case "up":
                        meleeInstance.transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case "down":
                        meleeInstance.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }
            }

            SafeDelay(attackDuration, () =>
            {
                if (meleeInstance != null) Destroy(meleeInstance);
            });
        }

        // ==================== Dash (usado por PlayerDashState) ====================

        public void SetDashEndCoroutine(Coroutine c) => dashEndCoroutine = c;
        public void SetGhostTimerCoroutine(Coroutine c) => ghostTimerCoroutine = c;
        public Coroutine GhostTimerCoroutine => ghostTimerCoroutine;

        /// <summary>
        /// genera una sombra del jugador mientras hace dash
        /// </summary>
        public void SpawnDashGhost()
        {
            if (dashGhostPrefab == null || spriteRenderer == null) return;

            GameObject ghost = Instantiate(dashGhostPrefab, transform.position, Quaternion.identity);
            var ghostRenderer = ghost.GetComponent<SpriteRenderer>();
            if (ghostRenderer != null)
            {
                ghostRenderer.sprite = spriteRenderer.sprite;
                ghostRenderer.flipX = spriteRenderer.flipX;
                ghostRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
                ghostRenderer.color = new Color(1f, 1f, 1f, 0.6f);
            }
            ghost.transform.localScale = transform.localScale;

            StartCoroutine(FadeAndDestroy(ghostRenderer, ghost, 0.25f));
        }

        private IEnumerator FadeAndDestroy(SpriteRenderer renderer, GameObject go, float duration)
        {
            float t = 0f;
            Color start = renderer != null ? renderer.color : Color.white;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (renderer != null)
                {
                    var c = start;
                    c.a = Mathf.Lerp(start.a, 0f, t / duration);
                    renderer.color = c;
                }
                yield return null;
            }
            Destroy(go);
        }

    }
}