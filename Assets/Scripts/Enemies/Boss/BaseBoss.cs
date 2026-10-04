using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using State;
using player;
using Managers;
using Level;

namespace Enemies
{

    /// <summary>
    /// Puertas/pisos que el boss puede abrir al morir.
    /// Implementa esta interfaz en tu script de puerta/piso existente
    /// (o dime el nombre real del método y lo ajusto).
    /// </summary>
    public interface IOpenable
    {
        void OpenDoor();
        void CloseDoor();
    }

    /// <summary>
    /// Plataformas que se pueden desactivar al recibir un puñetazo.
    /// Implementa esta interfaz en tu script de plataforma existente.
    /// </summary>
    public interface IPunchDeactivatable
    {
        void DeactivateByPunch();
    }

    /// <summary>
    /// Componente que se pone en cada objeto de ataque del boss (bola de fuego, puño, carámbano...).
    /// Sustituye a las propiedades dinámicas que Phaser le añadía al sprite (isProjectile, isPlatformPunch...).
    /// </summary>
    public class BossAttackObject : MonoBehaviour
    {
        public BaseBoss boss;
        public bool isProjectile = true;          // si es false, no se autodestruye al golpear al jugador
        public bool isPlatformPunch = false;      // marca los puños que rompen plataformas
        public bool destroyOnPlatform = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (boss == null) return;

            if (other.CompareTag("Player"))
            {
                boss.AttackCollisionWithPlayer(gameObject, isProjectile);
            }
            else if (other.CompareTag("Platform"))
            {
                var platform = other.GetComponent<IPunchDeactivatable>();
                boss.AttackCollisionWithPlatform(gameObject, platform, isPlatformPunch || destroyOnPlatform);
            }
        }
    }

    /// <summary>
    /// Clase base para todos los jefes del juego.
    /// </summary>
    public class BaseBoss : MonoBehaviour
    {
        [Header("Componentes (por Inspector, nunca GetComponent)")]
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected BoxCollider2D bodyCollider;

        [Header("Puertas")]
        [Tooltip("Puertas que desaparecen (SetActive false) cuando el jefe muere")]
        [SerializeField] protected List<Door> doors = new List<Door>();

        // se asignan desde script (SetDoors / SetFinalDoor / SetPlatforms)
        protected List<GameObject> floors = new List<GameObject>();
        protected GameObject finalDoor;
        protected List<GameObject> platforms = new List<GameObject>();

        [Header("Stats")]
        public float health = 10f;
        public float maxHealth = 10f;
        public float damage = 1f;

        [Header("Cooldowns (segundos)")]
        public float startCooldown = 2f;
        public float attackCooldown = 0f;
        public float minCooldown = 1f;
        public float maxCooldown = 1.5f;

        public bool isActivated = false;
        public bool notdead = true;
        public int phase = 1;

        public List<string> availableStates = new List<string>();
        public string bossName = "";

        // elementos visuales opcionales usados por applyDamageEffect (garras, máscara)
        public SpriteRenderer bossMask;
        public GameObject leftClaw;
        public GameObject rightClaw;
        protected SpriteRenderer leftClawRenderer;
        protected SpriteRenderer rightClawRenderer;
        public bool clawsActive = false;

        public StateMachine<BaseBoss> stateMachine;
        private readonly List<GameObject> bossAttacks = new List<GameObject>();

        public Rigidbody2D Rigidbody => rb;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        protected player.Player player => global::player.Player.Instance;
        public player.Player Player => global::player.Player.Instance;

        /// <summary>
        /// equivalente al constructor de BaseBoss. Las subclases (BossAngry, BossSad...)
        /// hacen "protected override void Awake() { base.Awake(); ... }" e inicializan ahí
        /// sus stats, animaciones y estados, igual que hacían tras el super() en JS.
        /// </summary>
        protected virtual void Awake()
        {
            isActivated = false;
            notdead = true;
            phase = 1;

            stateMachine = new StateMachine<BaseBoss>(this, "boss");

            SetupDefaultPhysics();
            SetupInactiveState();
            stateMachine.SetState("inactive");

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            if (notdead && isActivated)
            {
                stateMachine.Step(Time.time, Time.deltaTime);
            }
        }

        /// <summary>
        /// configura la física por defecto del jefe (quieto, inmóvil por colisiones)
        /// </summary>
        protected void SetupDefaultPhysics()
        {
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic; // equivalente a immovable + body.moves = false
        }

        protected void SetupInactiveState()
        {
            stateMachine.AddState("inactive", new BossInactiveState());
        }

        /// <summary>
        /// aplica daño al jefe
        /// </summary>
        public void TakeDamage(float dmg)
        {
            if (!isActivated || !notdead) return;

            health -= dmg;
            ApplyDamageEffect();

            if (health <= 0f) NextPhase();
        }

        /// <summary>
        /// aplica efecto visual de daño (tint + parpadeo) a todos los elementos visuales del boss
        /// </summary>
        protected void ApplyDamageEffect()
        {
            var visualElements = new List<SpriteRenderer> { spriteRenderer, bossMask, leftClawRenderer, rightClawRenderer }
                .Where(el => el != null && el.enabled)
                .ToList();

            StartCoroutine(DamageFlickerRoutine(visualElements));
        }

        private IEnumerator DamageFlickerRoutine(List<SpriteRenderer> elements)
        {
            Color tint = GetDamageTintColor();
            var originalColors = elements.Select(e => e.color).ToList();

            foreach (var e in elements) e.color = tint;

            for (int i = 0; i < 3; i++)
            {
                yield return FadeAlpha(elements, 0.5f, 0.05f);
                yield return FadeAlpha(elements, 1f, 0.05f);
            }

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].color = originalColors[i];
            }
        }

        private IEnumerator FadeAlpha(List<SpriteRenderer> elements, float targetAlpha, float duration)
        {
            float t = 0f;
            var startAlphas = elements.Select(e => e.color.a).ToList();
            while (t < duration)
            {
                t += Time.deltaTime;
                for (int i = 0; i < elements.Count; i++)
                {
                    var c = elements[i].color;
                    c.a = Mathf.Lerp(startAlphas[i], targetAlpha, t / duration);
                    elements[i].color = c;
                }
                yield return null;
            }
        }

        /// <summary>
        /// color del tint al recibir daño (rojo por defecto, cada jefe lo sobreescribe)
        /// </summary>
        public virtual Color GetDamageTintColor() => Color.red;

        /// <summary>
        /// colisión de un ataque del boss con el jugador (llamado desde BossAttackObject)
        /// </summary>
        public void AttackCollisionWithPlayer(GameObject attack, bool isProjectile)
        {
            if (attack == null || !attack.activeInHierarchy || Player == null) return;

            int dir = Player.transform.position.x < attack.transform.position.x ? -1 : 1;
            Player.TakeDamage(damage, dir);

            // solo destruir proyectiles, no advertencias
            if (isProjectile) Destroy(attack);
        }

        /// <summary>
        /// colisión de un ataque del boss con una plataforma (llamado desde BossAttackObject)
        /// </summary>
        public void AttackCollisionWithPlatform(GameObject attack, IPunchDeactivatable platform, bool shouldDeactivate)
        {
            if (attack == null || !attack.activeInHierarchy) return;

            if (shouldDeactivate)
            {
                platform?.DeactivateByPunch();
                Destroy(attack);
            }
        }

        /// <summary>
        /// avanza a la siguiente fase (las subclases lo sobreescriben)
        /// </summary>
        public virtual void NextPhase() { }

        /// <summary>
        /// inicia un estado aleatorio de entre los disponibles
        /// </summary>
        public void StartRandomState()
        {
            if (!isActivated || availableStates.Count == 0) return;
            string randomState = availableStates[UnityEngine.Random.Range(0, availableStates.Count)];
            stateMachine.SetState(randomState);
        }

        /// <summary>
        /// selecciona el siguiente estado del jefe
        /// </summary>
        public void SelectNextState()
        {
            if (!isActivated)
            {
                stateMachine.SetState("inactive");
                return;
            }
            GenerateNewCooldown();
            stateMachine.SetState("cooldown");
        }

        /// <summary>
        /// genera un nuevo tiempo de cooldown aleatorio
        /// </summary>
        public void GenerateNewCooldown()
        {
            attackCooldown = UnityEngine.Random.Range(minCooldown, maxCooldown);
        }

        /// <summary>
        /// maneja la muerte del jefe
        /// </summary>
        public virtual void Die()
        {
            // TODO: sacudida/flash de cámara (equivalente a scene.cameras.main.shake/flash de Phaser).
            // Conéctalo a tu propio sistema de cámara (Cinemachine Impulse, etc.)

            notdead = false;
            isActivated = false;

            // desactivar las puertas del jefe (lo primero, para que nada posterior lo impida)
            foreach (var door in doors)
            {
                if (door != null) door.gameObject.SetActive(false);
            }

            CleanupAllWarnings();
            DestroyAllAttackObjects();

            stateMachine.CurrentState?.Exit(this);
            stateMachine.SetState("inactive");

            // abrir pisos
            foreach (var floorObj in floors)
            {
                if (floorObj == null) continue;
                var openable = floorObj.GetComponent<IOpenable>();
                if (openable != null) openable.OpenDoor();
            }

            if (!string.IsNullOrEmpty(bossName)) PlayerDataManager.KillBoss(bossName);

            // TODO: emitir evento 'bossDefeated' para actualizar la UI (usa UnityEvent o tu propio EventBus)

            gameObject.SetActive(false);
            if (spriteRenderer != null) spriteRenderer.enabled = false;

            if (bossMask != null) Destroy(bossMask.gameObject);
            bossMask = null;
            DestroyClaws();
        }

        /// <summary>
        /// asigna puertas y pisos al jefe
        /// </summary>
        public void SetDoors(List<Door> doorsList, List<GameObject> floorsList = null)
        {
            doors = doorsList;
            floors = floorsList ?? new List<GameObject>();
        }

        public void SetFinalDoor(GameObject door) => finalDoor = door;

        /// <summary>
        /// activa el jefe
        /// </summary>
        /// <summary>
        /// arranca la secuencia del jefe (las subclases con intro animada la sobreescriben;
        /// por defecto activa el jefe directamente sin intro)
        /// </summary>
        public virtual void PlayIntro()
        {
            SetLife();
        }

        public virtual void SetLife()
        {
            if (PlayerDataManager.IsBossDefeated(bossName))
            {
                HandleAlreadyDefeated();
                return;
            }

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            gameObject.SetActive(true);
            isActivated = true;

            GenerateNewCooldown();
            stateMachine.SetState("cooldown");
        }

        /// <summary>
        /// maneja el caso cuando el jefe ya fue derrotado previamente
        /// </summary>
        public void HandleAlreadyDefeated()
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            gameObject.SetActive(false);
            isActivated = false;
            if (bossMask != null) bossMask.enabled = false;

            foreach (var door in doors)
            {
                if (door != null) door.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// limpia todas las advertencias visuales del estado actual
        /// </summary>
        public void CleanupAllWarnings()
        {
            (stateMachine.CurrentState as BaseBossAttackState)?.DestroyAllWarnings();
        }

        /// <summary>
        /// destruye todos los objetos de ataque activos del jefe
        /// </summary>
        public void DestroyAllAttackObjects()
        {
            foreach (var attack in bossAttacks)
            {
                if (attack != null) Destroy(attack);
            }
            bossAttacks.Clear();
        }

        /// <summary>
        /// agrega un ataque a la lista de ataques activos (equivalente al grupo bossAttacks de Phaser)
        /// </summary>
        public void AddAttack(GameObject attack)
        {
            bossAttacks.Add(attack);

            // asegúrate de que el prefab del ataque ya trae BossAttackObject + Collider2D (trigger) + Rigidbody2D;
            // si no lo trae, se lo añadimos aquí para no tener que tocar cada prefab a mano
            var attackObj = attack.GetComponent<BossAttackObject>();
            if (attackObj == null) attackObj = attack.AddComponent<BossAttackObject>();
            attackObj.boss = this;
        }

        /// <summary>
        /// agrega un estado a la máquina de estados
        /// </summary>
        public void AddState(string name, BaseState<BaseBoss> state)
        {
            stateMachine.AddState(name, state);
        }

        /// <summary>
        /// crea garras para el jefe (instancia un prefab ya preparado, no genera el sprite desde cero)
        /// </summary>
        public void CreateClaws(GameObject clawPrefab, float offsetX = 3.8f)
        {
            if (clawPrefab == null) return;

            var leftObj = Instantiate(clawPrefab, transform.position + new Vector3(-offsetX, -0.5f, 0f), Quaternion.identity);
            var rightObj = Instantiate(clawPrefab, transform.position + new Vector3(offsetX, -0.5f, 0f), Quaternion.identity);

            leftClaw = leftObj;
            rightClaw = rightObj;
            leftClawRenderer = leftObj.GetComponent<SpriteRenderer>();
            rightClawRenderer = rightObj.GetComponent<SpriteRenderer>();

            AddAttack(leftObj);
            AddAttack(rightObj);

            clawsActive = true;
        }

        /// <summary>
        /// destruye las garras del jefe
        /// </summary>
        public void DestroyClaws()
        {
            if (leftClaw != null) Destroy(leftClaw);
            if (rightClaw != null) Destroy(rightClaw);
            leftClaw = rightClaw = null;
            leftClawRenderer = rightClawRenderer = null;
            clawsActive = false;
        }

        /// <summary>
        /// establece la escala y el tamaño del collider del jefe (equivalente a setScaleAndBody de Phaser)
        /// </summary>
        public void SetScaleAndBody(float scale, float widthDivisor = 35f, float heightDivisor = 35f,
            float offsetXDivisor = 9.9f, float offsetYDivisor = 10.5f)
        {
            transform.localScale = new Vector3(scale, scale, 1f);

            if (bodyCollider != null && spriteRenderer != null)
            {
                float w = spriteRenderer.sprite.bounds.size.x * scale;
                float h = spriteRenderer.sprite.bounds.size.y * scale;
                bodyCollider.size = new Vector2(w / widthDivisor, h / heightDivisor);
                bodyCollider.offset = new Vector2(w / offsetXDivisor, h / offsetYDivisor);
            }
        }

        /// <summary>
        /// asigna plataformas al jefe (para la colisión ataque-plataforma)
        /// </summary>
        public void SetPlatforms(List<GameObject> platformList)
        {
            platforms = platformList;
        }

        // ==================== Helpers (equivalentes a scene.time.delayedCall / addEvent de Phaser) ====================

        public Coroutine SafeDelay(float delaySeconds, Action callback)
        {
            return StartCoroutine(SafeDelayRoutine(delaySeconds, callback));
        }

        private IEnumerator SafeDelayRoutine(float delaySeconds, Action callback)
        {
            yield return new WaitForSeconds(delaySeconds);
            callback?.Invoke();
        }

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
    }

    /// <summary>
    /// estado inactivo del jefe (equivalente al objeto plano { enter, step, exit } de setupInactiveState)
    /// </summary>
    public class BossInactiveState : BaseState<BaseBoss>
    {
        public override void Enter(BaseBoss boss, object data = null) { }
        public override void Execute(BaseBoss boss, float time, float delta) { }
        public override void Exit(BaseBoss boss) { }
    }
}