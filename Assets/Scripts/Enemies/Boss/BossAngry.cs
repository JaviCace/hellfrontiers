using System.Collections;
using UnityEngine;

namespace Enemies
{

    /// <summary>
    /// Jefe de la emoción Ira.
    /// </summary>
    public class BossAngry : BaseBoss
    {
        [Header("Stats específicos de Ira")]
        [Tooltip("Altura del puño horizontal: distancia hacia abajo desde el jefe, en unidades de mundo")]
        [Range(0f, 10f)] public float distanceToFloor = 1f;
        public float fireballSpeed = 4.5f;          // unidades por segundo
        public float punchYSpeed = 12f;
        public float punchXSpeed = 6f;

        [Header("Bolas de fuego (bullet hell)")]
        [Tooltip("Número de bolas que se disparan en cada ataque")]
        [Min(1)] public int fireballCount = 30;
        [Tooltip("Segundos entre una bola y la siguiente")]
        [Min(0.01f)] public float fireballInterval = 0.12f;
        [Tooltip("Ángulo mínimo. 0° = derecha, 90° = recto hacia abajo, 180° = izquierda")]
        [Range(1f, 179f)] public float fireballMinAngle = 90f;
        [Tooltip("Ángulo máximo. 135° = diagonal hacia abajo a la izquierda")]
        [Range(1f, 179f)] public float fireballMaxAngle = 135f;
        [Tooltip("Desplazamiento respecto al jefe desde donde sale cada bola")]
        public Vector2 fireballSpawnOffset = Vector2.zero;

        [Header("Prefabs de ataque (con Rigidbody2D + Collider2D trigger)")]
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private GameObject punchPrefab;
        public GameObject FireballPrefab => fireballPrefab;
        public GameObject PunchPrefab => punchPrefab;

        protected override void Awake()
        {

            health = 3;
            maxHealth = 3;
            damage = 1f;
            startCooldown = 2f;
            minCooldown = 1f;
            maxCooldown = 1.5f;
            availableStates = new System.Collections.Generic.List<string> { "punch", "fireball" };
            bossName = "anger";

            base.Awake();               // inicializar BaseBoss (stateMachine, física, estado inactivo)

            // NOTA: setScaleAndBody ya no hace falta aquí si el prefab ya viene con el
            // Collider2D del tamaño correcto; llama a SetScaleAndBody(...) si necesitas
            // replicar el cálculo exacto que tenías en Phaser.

            SetupStates();
        }

        /// <summary>
        /// reproduce la intro del jefe Ira
        /// </summary>
        public override void PlayIntro()
        {
            gameObject.SetActive(true);
            if (SpriteRenderer != null) SpriteRenderer.enabled = true;

            StartCoroutine(IntroRoutine());
        }

        private IEnumerator IntroRoutine()
        {
            yield return WaitSteps(3);

            // TODO: sacudida de cámara (scene.cameras.main.shake(2000, 0.05) en el original)
            yield return WaitSteps(3);

            SetLife();
            // TODO: emitir evento 'bossIntroFinished' (usa UnityEvent o tu propio EventBus)
        }

        /// <summary>
        /// espera fija (0.5 s por paso) que sustituye a la duración de las animaciones
        /// </summary>
        private IEnumerator WaitSteps(int steps)
        {
            yield return new WaitForSeconds(0.5f * Mathf.Max(1, steps));
        }

        /// <summary>
        /// configura los estados específicos del jefe Ira
        /// </summary>
        private void SetupStates()
        {
            AddState("punch", new BossAngryPunchState());
            AddState("fireball", new BossAngryFireBallState());
            AddState("punchPlatform", new BossAngryPunchPlatformState());
            AddState("cooldown", new BossAngryCooldownState());
        }

        /// <summary>
        /// avanza a la siguiente fase del jefe Ira
        /// </summary>
        public override void NextPhase()
        {
            if (phase == 1)
            {
                phase = 2;
                health = maxHealth * 1.5f;
                availableStates.Add("punchPlatform");
                minCooldown = 0.75f;
                maxCooldown = 1.25f;
                HandlePhaseTransition();
            }
            else
            {
                Die();
            }
        }

        /// <summary>
        /// maneja la transición entre fases
        /// </summary>
        private void HandlePhaseTransition()
        {
            CleanupAllWarnings();
            DestroyAllAttackObjects();
            stateMachine?.SetState("inactive");

            // el GameObject se queda activo (solo se oculta): con un objeto inactivo la corrutina
            // de la transición no puede arrancar y el jefe no volvería nunca ni llegaría a morir
            if (SpriteRenderer != null) SpriteRenderer.enabled = false;
            if (bodyCollider != null) bodyCollider.enabled = false;
            isActivated = false;

            StartCoroutine(PhaseTransitionRoutine());
        }

        private IEnumerator PhaseTransitionRoutine()
        {
            // el jefe desaparece 2 segundos y vuelve a aparecer, sin transición
            yield return new WaitForSeconds(2f);

            if (SpriteRenderer != null) SpriteRenderer.enabled = true;
            if (bodyCollider != null) bodyCollider.enabled = true;

            isActivated = true;
            GenerateNewCooldown();
            stateMachine.SetState("cooldown");
        }

        /// <summary>
        /// color del tint para el daño de Ira
        /// </summary>
        public override Color GetDamageTintColor() => new Color(1f, 0f, 0f);

        /// <summary>
        /// con el jefe seleccionado, dibuja en la Scene view la altura del puño horizontal (línea roja)
        /// y el origen de las bolas de fuego con su rango de ángulos (naranja)
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            float y = transform.position.y - distanceToFloor;
            float x = transform.position.x;
            Gizmos.DrawLine(new Vector3(x - 10f, y, 0f), new Vector3(x + 10f, y, 0f));

            // origen de las bolas de fuego (círculo) y límites del rango de ángulos (dos líneas)
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Vector3 origin = transform.position + (Vector3)fireballSpawnOffset;
            Gizmos.DrawWireSphere(origin, 0.2f);
            DrawFireballRay(origin, fireballMinAngle);
            DrawFireballRay(origin, fireballMaxAngle);
        }

        private static void DrawFireballRay(Vector3 origin, float angle)
        {
            // misma convención que el ataque: 0° = derecha, 90° = abajo, 180° = izquierda
            float rad = Mathf.Clamp(angle, 1f, 179f) * Mathf.Deg2Rad;
            Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Cos(rad), -Mathf.Sin(rad), 0f) * 4f);
        }
    }
}