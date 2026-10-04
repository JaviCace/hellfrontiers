using System.Collections;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Jefe de la emoción Tristeza.
/// </summary>
public class BossSad : BaseBoss
{
    [Header("Stats específicos de Tristeza")]
    public float distanceToFloor = 250f;
    public float icicleSpeed = 900f;
    public float waterBallSpeed = 200f;
    public float radialSpeed = 400f;

    [Header("Prefabs de ataque (con Rigidbody2D + Collider2D trigger)")]
    [SerializeField] private GameObject iciclePrefab;
    [SerializeField] private GameObject waterBallPrefab;
    public GameObject IciclePrefab => iciclePrefab;
    public GameObject WaterBallPrefab => waterBallPrefab;

    protected override void Awake()
    {
        health = 3;
        maxHealth = 3;
        damage = 1f;
        startCooldown = 2f;
        minCooldown = 2f;
        maxCooldown = 2.5f;
        availableStates = new System.Collections.Generic.List<string> { "radial", "waterball" };
        bossName = "sadness";

        base.Awake();

        SetupStates();
    }

    /// <summary>
    /// reproduce la intro del jefe Tristeza
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

        yield return new WaitForSeconds(0.8f);

        // TODO: sacudida de cámara (shake(2000, 0.05))
        yield return WaitSteps(1);

        yield return WaitSteps(2);

        SetLife();
        // TODO: emitir evento 'bossIntroFinished'
    }

    /// <summary>
    /// espera fija (0.5 s por paso) que sustituye a la duración de las animaciones
    /// </summary>
    private IEnumerator WaitSteps(int steps)
    {
        yield return new WaitForSeconds(0.5f * Mathf.Max(1, steps));
    }

    /// <summary>
    /// configura los estados específicos del jefe Tristeza
    /// </summary>
    private void SetupStates()
    {
        AddState("icicle", new BossSadIcicleState());
        AddState("radial", new BossSadRadialState());
        AddState("waterball", new BossSadWaterBallState());
        AddState("cooldown", new BossSadCooldownState());
    }

    /// <summary>
    /// color del tint para el daño de Tristeza
    /// </summary>
    public override Color GetDamageTintColor() => new Color(0f, 0f, 1f);

    /// <summary>
    /// avanza a la siguiente fase del jefe Tristeza
    /// </summary>
    public override void NextPhase()
    {
        if (phase == 1)
        {
            phase = 2;
            health = maxHealth * 1.5f;
            availableStates.Add("icicle");
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
    /// maneja la transición entre fases del jefe Tristeza
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
}
}
