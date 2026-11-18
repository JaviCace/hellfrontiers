using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Ataque de salto del jefe Tutorial: sube, marca en el suelo donde va a caer (donde está el jugador,
/// sin salirse de su sala) y cae sobre esa posición.
///   warning  = sube (lift)
///   attack   = cae (fall)
///   cooldown = efecto de aterrizaje y pausa antes del siguiente ataque
/// </summary>
public class BossTutorialJumpAttackState : BaseBossAttackState
{
    private BossTutorial tutorial;
    private float groundY;
    private float targetX;
    private float leftLimit;
    private float rightLimit;
    private bool landed;
    private Coroutine moveRoutine;

    public BossTutorialJumpAttackState() : base(new BossAttackConfig
    {
        AttackName = "Ataque de Salto",
        Phases = new List<string> { "warning", "attack", "cooldown" },
        WarningDuration = 1.4f,
        AttackDuration = 0.8f,
        CooldownDuration = 2.4f   // 1.4 s de aviso tras caer + 1 s de cierre
    })
    { }

    public override void Enter(BaseBoss context, object data = null)
    {
        tutorial = (BossTutorial)context;
        tutorial.KeepInsideRoom();
        groundY = context.transform.position.y;
        landed = false;

        // límites de la sala (se calculan ahora, con el jefe todavía dentro de ella)
        tutorial.GetHorizontalLimits(out leftLimit, out rightLimit);

        base.Enter(context, data);
    }

    /// <summary>
    /// fase warning: el jefe sube
    /// </summary>
    protected override void CreateWarning()
    {
        Vector3 from = boss.transform.position;
        Vector3 to = new Vector3(from.x, tutorial.ClampHeight(groundY + tutorial.jumpHeight), from.z); // sin salirse del techo de la sala
        moveRoutine = boss.StartCoroutine(MoveTo(from, to, config.WarningDuration, p => 1f - (1f - p) * (1f - p))); // Quad.easeOut
    }

    /// <summary>
    /// fase attack: el jefe cae sobre el jugador
    /// </summary>
    protected override void ExecuteAttack()
    {
        // posición del jugador, sin salirse de la sala
        float playerX = tutorial.Player != null ? tutorial.Player.transform.position.x : boss.transform.position.x;
        targetX = Mathf.Clamp(playerX, leftLimit, rightLimit);

        if (boss.SpriteRenderer != null) boss.SpriteRenderer.flipX = targetX > boss.transform.position.x;

        CreateGroundWarning();

        Vector3 from = boss.transform.position;
        Vector3 to = new Vector3(targetX, groundY, from.z);
        if (moveRoutine != null) boss.StopCoroutine(moveRoutine);
        moveRoutine = boss.StartCoroutine(MoveTo(from, to, config.AttackDuration, p => p * p)); // Quad.easeIn
    }

    private void CreateGroundWarning()
    {
        var warningRect = CreateWarningRectangle(targetX, groundY, 1.2f, 1.2f, Color.red, 0.45f);
        CreatePulseEffect(new List<WarningVisual> { warningRect }, 0.4f, 0.3f, 0.6f);
    }

    public override void Execute(BaseBoss context, float time, float delta)
    {
        base.Execute(context, time, delta);

        // al acabar la caída (empieza la fase cooldown) el jefe aterriza
        if (!landed && currentPhase == "cooldown") OnLand();
    }

    private IEnumerator MoveTo(Vector3 from, Vector3 to, float duration, System.Func<float, float> ease)
    {
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = ease(Mathf.Clamp01(t / duration));
            boss.transform.position = Vector3.Lerp(from, to, p);
            yield return null;
        }
        boss.transform.position = to;
        moveRoutine = null;
    }

    private void OnLand()
    {
        landed = true;
        DestroyAllWarnings(); // quita la marca del suelo

        if (moveRoutine != null) boss.StopCoroutine(moveRoutine);
        moveRoutine = null;

        var pos = boss.transform.position;
        boss.transform.position = new Vector3(targetX, groundY, pos.z);

        // si cae encima del jugador, le golpea
        tutorial.TryHitPlayer();

        CreateLandingEffect();
    }

    /// <summary>
    /// círculo de impacto y tres ondas expansivas
    /// </summary>
    private void CreateLandingEffect()
    {
        Vector3 pos = new Vector3(targetX, groundY, 0f);

        var impact = BossFx.CreateCircle(pos, 0.8f, new Color(1f, 0f, 0f, 0.3f));
        Object.Destroy(impact, 0.5f);

        for (int i = 0; i < 3; i++)
        {
            var wave = BossFx.CreateCircle(pos, 0.4f, new Color(1f, 0.27f, 0.27f, 0.4f - i * 0.1f));
            float toScale = 2f + i * 0.5f;
            boss.StartCoroutine(BossFx.ExpandAndFade(wave, toScale, 0.3f, i * 0.1f));
            Object.Destroy(wave, 0.3f + i * 0.1f + 0.1f); // por si la corrutina se corta
        }

        // TODO: sacudida de cámara (shake(200, 0.01))
    }

    public override void Exit(BaseBoss context)
    {
        base.Exit(context);

        if (moveRoutine != null) boss.StopCoroutine(moveRoutine);
        moveRoutine = null;

        // volver a la altura del suelo
        var pos = boss.transform.position;
        boss.transform.position = new Vector3(pos.x, groundY, pos.z);
    }
}
}
