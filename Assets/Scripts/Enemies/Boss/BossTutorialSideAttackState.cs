using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Ataque lateral del jefe Tutorial: avisa con una banda roja y cruza su sala de puerta a puerta.
/// Si completa el barrido sin tocar al jugador, el propio jefe se hace daño.
/// </summary>
public class BossTutorialSideAttackState : BaseBossAttackState
{
    private BossTutorial tutorial;
    private float initialY;
    private float leftLimit;
    private float rightLimit;
    private string direction = "";
    private Coroutine sweepRoutine;

    public BossTutorialSideAttackState() : base(new BossAttackConfig
    {
        AttackName = "Ataque Lateral",
        Phases = new List<string> { "warning", "attack", "cooldown" },
        WarningDuration = 1f,
        AttackDuration = 2f,
        CooldownDuration = 2f
    })
    { }

    public override void Enter(BaseBoss context, object data = null)
    {
        tutorial = (BossTutorial)context;
        tutorial.ResetHitFlag();
        tutorial.KeepInsideRoom();
        initialY = context.transform.position.y;

        // límites de la sala (se calculan ahora, con el jefe todavía dentro de ella)
        tutorial.GetHorizontalLimits(out leftLimit, out rightLimit);

        base.Enter(context, data);
    }

    protected override void CreateWarning()
    {
        float roomCenter = (leftLimit + rightLimit) / 2f;
        direction = boss.transform.position.x < roomCenter ? "right" : "left";

        // banda roja a lo ancho de la sala, a la altura del jefe
        CreateWarningRectangle(roomCenter, initialY, (rightLimit - leftLimit) + 1f, 1.2f, Color.red, 0.4f);
    }

    protected override void ExecuteAttack()
    {
        sweepRoutine = boss.StartCoroutine(SideSweep());
    }

    private IEnumerator SideSweep()
    {
        bool toRight = direction == "right";
        float fromX = toRight ? leftLimit : rightLimit;
        float toX = toRight ? rightLimit : leftLimit;

        boss.transform.position = new Vector3(fromX, initialY, boss.transform.position.z);
        if (boss.SpriteRenderer != null) boss.SpriteRenderer.flipX = toRight;

        float duration = Mathf.Max(0.01f, config.AttackDuration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = -(Mathf.Cos(Mathf.PI * p) - 1f) / 2f; // Sine.easeInOut

            boss.transform.position = new Vector3(Mathf.Lerp(fromX, toX, eased), initialY, boss.transform.position.z);
            yield return null;
        }

        sweepRoutine = null;

        // si no ha tocado al jugador en todo el barrido, el jefe se hace daño
        // (va lo último: puede cambiar de fase o morir y salir de este estado)
        if (!tutorial.HitPlayerThisSweep) boss.TakeDamage(tutorial.missDamage);
    }

    public override void Exit(BaseBoss context)
    {
        base.Exit(context);

        if (sweepRoutine != null) boss.StopCoroutine(sweepRoutine);
        sweepRoutine = null;

        tutorial.ResetHitFlag();
    }
}
}
