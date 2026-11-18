using System.Collections;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Jefe de la emoción Miedo.
/// </summary>
public class BossFear : BaseBoss, IFearBoss
{
    [Header("Ataque de tazas")]
    [Tooltip("Velocidad de caída de las tazas (unidades por segundo)")]
    [SerializeField] private float cupSpeed = 4.5f;
    [Tooltip("Número de tazas por ataque")]
    [Min(1)] [SerializeField] private int cupCount = 10;
    [Tooltip("Segundos entre una taza y la siguiente")]
    [Min(0.01f)] [SerializeField] private float cupInterval = 0.4f;

    [Header("Garras (ataque en X)")]
    [Tooltip("Separación horizontal inicial de las garras respecto al jefe")]
    [SerializeField] private float clawOffsetX = 3.8f;

    [Header("Máscara")]
    [Tooltip("Posición de la máscara respecto al jefe (solo si la máscara NO es hija del jefe)")]
    [SerializeField] private Vector2 maskOffset = new Vector2(0f, 2f);

    [Header("Prefabs de ataque (con Rigidbody2D + Collider2D trigger)")]
    [SerializeField] private GameObject cupPrefab;
    [SerializeField] private GameObject clawPrefab;

    public float CupSpeed => cupSpeed;
    public int CupCount => cupCount;
    public float CupInterval => cupInterval;
    public GameObject CupPrefab => cupPrefab;
    public GameObject ClawPrefab => clawPrefab;
    public float ClawOffsetX => clawOffsetX;

    protected override void Awake()
    {
        health = 35f;
        maxHealth = 35f;
        damage = 1f;
        startCooldown = 3f;
        minCooldown = 1.5f;
        maxCooldown = 2.5f;
        availableStates = new System.Collections.Generic.List<string> { "xAttack" };
        bossName = "fear";

        base.Awake();

        SetupStates();

        // la máscara solo se ve cuando el jefe está activo
        if (bossMask != null) bossMask.enabled = false;
    }

    private void SetupStates()
    {
        AddState("xAttack", new BossFearXAttackState());
        AddState("cupAttack", new BossFearCupAttackState());
        AddState("cooldown", new BossFearCooldownState());
    }

    /// <summary>
    /// reproduce la intro del jefe Miedo
    /// </summary>
    public override void PlayIntro()
    {
        gameObject.SetActive(true);
        if (SpriteRenderer != null) SpriteRenderer.enabled = true;
        if (bossMask != null) bossMask.enabled = true;

        // TODO: sacudida de cámara (shake(3000, 0.05))
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        SetLife();
        // TODO: emitir evento 'bossIntroFinished'
    }

    /// <summary>
    /// avanza a la siguiente fase del jefe Miedo
    /// </summary>
    public override void NextPhase()
    {
        if (phase == 1)
        {
            phase = 2;
            health = maxHealth + 3f;
            availableStates.Add("cupAttack");
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
        if (bossMask != null) bossMask.enabled = false;
        isActivated = false;
        DestroyClaws();

        StartCoroutine(PhaseTransitionRoutine());
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (SpriteRenderer != null) SpriteRenderer.enabled = true;
        if (bodyCollider != null) bodyCollider.enabled = true;
        if (bossMask != null) bossMask.enabled = true;

        isActivated = true;
        GenerateNewCooldown();
        stateMachine.SetState("cooldown");
    }

    protected override void Update()
    {
        base.Update();

        // la máscara sigue al jefe (si es hija suya ya lo hace sola)
        if (bossMask != null && bossMask.enabled && bossMask.transform.parent != transform)
        {
            bossMask.transform.position = transform.position + (Vector3)maskOffset;
        }
    }

    public override Color GetDamageTintColor() => new Color(1f, 0f, 0f);
}
}
