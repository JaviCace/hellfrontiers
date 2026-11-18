using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Enemies
{

/// <summary>
/// Jefe final: combina los ataques de los demás jefes.
/// En la fase 1 usa 3 ataques al azar; en la fase 2, todos.
/// </summary>
public class BossFinal : BaseBoss, IAngryBoss, ISadBoss, IFearBoss
{
    [Header("Final de juego")]
    [Tooltip("Nombre de la escena de victoria (debe estar en Build Settings). Vacío = no carga ninguna")]
    [SerializeField] private string winSceneName = "Win";
    [Tooltip("Segundos entre que muere el jefe y se carga la escena de victoria")]
    [SerializeField] private float winSceneDelay = 5f;

    [Header("Ira: puño y bolas de fuego")]
    [Tooltip("Altura del puño horizontal: distancia hacia abajo desde el jefe, en unidades de mundo")]
    [Range(0f, 10f)] [SerializeField] private float distanceToFloor = 2.5f;
    [SerializeField] private float fireballSpeed = 4.5f;
    [SerializeField] private float punchYSpeed = 10f;
    [SerializeField] private float punchXSpeed = 6f;
    [Min(1)] [SerializeField] private int fireballCount = 30;
    [Min(0.01f)] [SerializeField] private float fireballInterval = 0.12f;
    [Range(1f, 179f)] [SerializeField] private float fireballMinAngle = 90f;
    [Range(1f, 179f)] [SerializeField] private float fireballMaxAngle = 135f;
    [SerializeField] private Vector2 fireballSpawnOffset = Vector2.zero;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject punchPrefab;

    [Header("Miedo: tazas y garras")]
    [SerializeField] private float cupSpeed = 4.5f;
    [Min(1)] [SerializeField] private int cupCount = 10;
    [Min(0.01f)] [SerializeField] private float cupInterval = 0.4f;
    [SerializeField] private float clawOffsetX = 3.8f;
    [SerializeField] private GameObject cupPrefab;
    [SerializeField] private GameObject clawPrefab;

    [Header("Tristeza: carámbanos y bola de agua")]
    [SerializeField] private float icicleSpeed = 4.5f;
    [SerializeField] private float waterBallSpeed = 2f;
    [SerializeField] private float radialSpeed = 4f;
    [SerializeField] private GameObject iciclePrefab;
    [SerializeField] private GameObject waterBallPrefab;

    // IAngryBoss / ISadBoss / IFearBoss
    public float DistanceToFloor => distanceToFloor;
    public float FireballSpeed => fireballSpeed;
    public float PunchYSpeed => punchYSpeed;
    public float PunchXSpeed => punchXSpeed;
    public int FireballCount => fireballCount;
    public float FireballInterval => fireballInterval;
    public float FireballMinAngle => fireballMinAngle;
    public float FireballMaxAngle => fireballMaxAngle;
    public Vector2 FireballSpawnOffset => fireballSpawnOffset;
    public GameObject FireballPrefab => fireballPrefab;
    public GameObject PunchPrefab => punchPrefab;
    public float CupSpeed => cupSpeed;
    public int CupCount => cupCount;
    public float CupInterval => cupInterval;
    public GameObject CupPrefab => cupPrefab;
    public GameObject ClawPrefab => clawPrefab;
    public float ClawOffsetX => clawOffsetX;
    public float IcicleSpeed => icicleSpeed;
    public float WaterBallSpeed => waterBallSpeed;
    public float RadialSpeed => radialSpeed;
    public GameObject IciclePrefab => iciclePrefab;
    public GameObject WaterBallPrefab => waterBallPrefab;

    // todos los ataques disponibles para el jefe final
    private List<string> allStates;

    protected override void Awake()
    {
        health = 45f;
        maxHealth = 45f;
        damage = 1f;
        startCooldown = 1.5f;
        minCooldown = 0.8f;
        maxCooldown = 1f;
        bossName = "final";

        allStates = new List<string>
        {
            "fireball", "punch", "punchPlatform",
            "xAttack", "cupAttack",
            "icicle", "radial", "waterball"
        };
        availableStates = SelectRandomStates(3);

        base.Awake();

        SetupStates();
    }

    /// <summary>
    /// configura todos los estados del jefe final
    /// </summary>
    private void SetupStates()
    {
        AddState("fireball", new BossAngryFireBallState());
        AddState("punch", new BossAngryPunchState());
        AddState("punchPlatform", new BossAngryPunchPlatformState());
        AddState("xAttack", new BossFearXAttackState());
        AddState("cupAttack", new BossFearCupAttackState());
        AddState("icicle", new BossSadIcicleState());
        AddState("radial", new BossSadRadialState());
        AddState("waterball", new BossSadWaterBallState());
        AddState("cooldown", new BossFinalCooldownState());
    }

    /// <summary>
    /// selecciona estados aleatorios, sin repetir
    /// </summary>
    private List<string> SelectRandomStates(int count)
    {
        var pool = new List<string>(allStates);
        var picked = new List<string>();
        while (picked.Count < count && pool.Count > 0)
        {
            int i = Random.Range(0, pool.Count);
            picked.Add(pool[i]);
            pool.RemoveAt(i);
        }
        return picked;
    }

    /// <summary>
    /// reproduce la intro del jefe final
    /// </summary>
    public override void PlayIntro()
    {
        gameObject.SetActive(true);
        if (SpriteRenderer != null) SpriteRenderer.enabled = true;

        // TODO: sacudida y flash de cámara (shake(2000, 0.07), flash magenta)
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        yield return new WaitForSeconds(2f);

        SetLife();
        // TODO: emitir evento 'bossIntroFinished'
    }

    public override Color GetDamageTintColor() => new Color(1f, 0f, 1f);

    /// <summary>
    /// avanza a la siguiente fase del jefe final
    /// </summary>
    public override void NextPhase()
    {
        if (phase == 1)
        {
            phase = 2;
            health = maxHealth + 5f;
            availableStates = new List<string>(allStates);
            minCooldown = 0.6f;
            maxCooldown = 1f;
            HandlePhaseTransition();
        }
        else
        {
            Die();
        }
    }

    /// <summary>
    /// el jefe desaparece 2 segundos y vuelve a aparecer, sin transición
    /// </summary>
    private void HandlePhaseTransition()
    {
        CleanupAllWarnings();
        DestroyAllAttackObjects();
        stateMachine?.SetState("inactive");

        // el GameObject se queda activo (solo se oculta): con un objeto inactivo la corrutina
        // no podría arrancar y el jefe no volvería nunca
        if (SpriteRenderer != null) SpriteRenderer.enabled = false;
        if (bodyCollider != null) bodyCollider.enabled = false;
        isActivated = false;
        DestroyClaws();

        StartCoroutine(PhaseTransitionRoutine());
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (SpriteRenderer != null) SpriteRenderer.enabled = true;
        if (bodyCollider != null) bodyCollider.enabled = true;

        isActivated = true;
        GenerateNewCooldown();
        stateMachine.SetState("cooldown");
    }

    /// <summary>
    /// muerte del jefe final: tras unos segundos carga la escena de victoria
    /// </summary>
    public override void Die()
    {
        // TODO: sacudida y flash dorado de cámara (shake(2000, 0.05), flash(1500, 255, 215, 0))
        base.Die();

        // base.Die() desactiva este GameObject, así que la espera la hace otro objeto
        if (!string.IsNullOrEmpty(winSceneName))
        {
            new GameObject("WinSceneLoader").AddComponent<WinSceneLoader>().Begin(winSceneName, winSceneDelay);
        }
    }
}

/// <summary>
/// espera unos segundos y carga la escena indicada (vive en un objeto aparte para sobrevivir a la desactivación del jefe)
/// </summary>
public class WinSceneLoader : MonoBehaviour
{
    public void Begin(string sceneName, float delay)
    {
        StartCoroutine(Run(sceneName, delay));
    }

    private IEnumerator Run(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(sceneName);
    }
}
}
