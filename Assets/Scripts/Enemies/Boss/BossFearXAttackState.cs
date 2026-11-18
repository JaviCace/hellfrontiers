using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Estado de ataque en X del jefe Miedo (y el Final): dos garras cruzan la pantalla en diagonal.
/// El prefab de la garra se elige en el Inspector del jefe (IFearBoss.ClawPrefab).
/// </summary>
public class BossFearXAttackState : BaseBossAttackState
{
    // NOTA: las distancias de aquí abajo eran píxeles de Phaser (/100). Ajusta a tu escala.
    private const float WarningOffsetX = 3f;
    private const float WarningOffsetY = -1f;   // por debajo del jefe
    private const float StartOffsetX = 3.8f;
    private const float StartOffsetY = 1f;      // por encima del jefe
    private const float EndOffsetX = 3f;
    private const float EndOffsetY = -4f;       // por debajo del jefe
    private const float WaveAmplitude = 0.2f;

    private readonly List<GameObject> warningObjects = new List<GameObject>();
    private Coroutine flashRoutine;
    private Coroutine leftRoutine;
    private Coroutine rightRoutine;
    private Coroutine finishRoutine;
    private int completedClaws;

    public BossFearXAttackState() : base(new BossAttackConfig
    {
        AttackName = "Ataque en X",
        Phases = new List<string> { "warning", "attack", "cooldown" },
        WarningDuration = 1f,
        AttackDuration = 2f,
        CooldownDuration = 1f
    })
    { }

    public override void Enter(BaseBoss context, object data = null)
    {
        base.Enter(context, data);
        CreateClawsHidden();
    }

    /// <summary>
    /// crea las garras ocultas (aparecen al empezar el ataque) y evita que se destruyan al tocar al jugador
    /// </summary>
    private void CreateClawsHidden()
    {
        var fear = (IFearBoss)boss;
        boss.CreateClaws(fear.ClawPrefab, fear.ClawOffsetX);

        HideClaw(boss.leftClaw);
        HideClaw(boss.rightClaw);
    }

    private static void HideClaw(GameObject claw)
    {
        if (claw == null) return;

        var attackObj = claw.GetComponent<BossAttackObject>();
        if (attackObj != null) attackObj.isProjectile = false; // la garra no desaparece al golpear

        claw.SetActive(false);
    }

    /// <summary>
    /// crea las advertencias en forma de X
    /// </summary>
    protected override void CreateWarning()
    {
        Vector3 c = boss.transform.position;
        var color = new Color(1f, 0f, 0f, 0.5f);

        // en pantalla la "\" izquierda baja hacia la derecha y la "/" derecha baja hacia la izquierda
        warningObjects.Add(BossFx.CreateQuad(c + new Vector3(-WarningOffsetX, WarningOffsetY, 0f), new Vector2(3f, 0.6f), color, -45f));
        warningObjects.Add(BossFx.CreateQuad(c + new Vector3(WarningOffsetX, WarningOffsetY, 0f), new Vector2(3f, 0.6f), color, 45f));

        flashRoutine = boss.StartCoroutine(FlashWarnings());
    }

    private IEnumerator FlashWarnings()
    {
        // parpadeo de alpha 0.3 <-> 0.7 (cada 0.2 s)
        while (true)
        {
            yield return FadeWarnings(0.3f, 0.7f, 0.2f);
            yield return FadeWarnings(0.7f, 0.3f, 0.2f);
        }
    }

    private IEnumerator FadeWarnings(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            foreach (var w in warningObjects) BossFx.SetAlpha(w, a);
            yield return null;
        }
    }

    protected override void ExecuteAttack()
    {
        MoveClawsInXPattern();
    }

    private void MoveClawsInXPattern()
    {
        if (boss.leftClaw == null || boss.rightClaw == null)
        {
            // intentar recrearlas
            CreateClawsHidden();

            if (boss.leftClaw == null || boss.rightClaw == null)
            {
                Debug.LogError("[BossFear] No se pudieron crear las garras, se salta el ataque");
                boss.SelectNextState();
                return;
            }
        }

        Vector3 b = boss.transform.position;
        Vector3 startLeft = new Vector3(b.x - StartOffsetX, b.y + StartOffsetY, 0f);
        Vector3 startRight = new Vector3(b.x + StartOffsetX, b.y + StartOffsetY, 0f);
        Vector3 endLeft = new Vector3(b.x + EndOffsetX, b.y + EndOffsetY, 0f);
        Vector3 endRight = new Vector3(b.x - EndOffsetX, b.y + EndOffsetY, 0f);

        boss.leftClaw.transform.position = startLeft;
        boss.rightClaw.transform.position = startRight;
        boss.leftClaw.SetActive(true);
        boss.rightClaw.SetActive(true);

        completedClaws = 0;
        leftRoutine = boss.StartCoroutine(MoveClaw(boss.leftClaw, startLeft, endLeft, config.AttackDuration));
        rightRoutine = boss.StartCoroutine(MoveClaw(boss.rightClaw, startRight, endRight, config.AttackDuration));
    }

    /// <summary>
    /// X con ease in-out, Y lineal con una pequeña ondulación (igual que el tween de Phaser)
    /// </summary>
    private IEnumerator MoveClaw(GameObject claw, Vector3 start, Vector3 end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            if (claw == null) yield break;

            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = p < 0.5f ? 2f * p * p : 1f - Mathf.Pow(-2f * p + 2f, 2f) / 2f;

            float x = Mathf.Lerp(start.x, end.x, eased);
            float y = Mathf.Lerp(start.y, end.y, p) + Mathf.Sin(p * Mathf.PI * 2f) * WaveAmplitude;
            claw.transform.position = new Vector3(x, y, 0f);
            yield return null;
        }

        if (claw != null) claw.transform.position = end;

        // cuando terminan las dos garras, se destruyen un instante después
        completedClaws++;
        if (completedClaws >= 2) finishRoutine = boss.StartCoroutine(DestroyClawsAfter(0.3f));
    }

    private IEnumerator DestroyClawsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        boss.DestroyClaws();
    }

    public override void DestroyAllWarnings()
    {
        base.DestroyAllWarnings();

        if (flashRoutine != null) boss.StopCoroutine(flashRoutine);
        flashRoutine = null;

        foreach (var w in warningObjects) if (w != null) Object.Destroy(w);
        warningObjects.Clear();
    }

    public override void Exit(BaseBoss context)
    {
        base.Exit(context);
        DestroyAllWarnings();

        if (leftRoutine != null) boss.StopCoroutine(leftRoutine);
        if (rightRoutine != null) boss.StopCoroutine(rightRoutine);
        if (finishRoutine != null) boss.StopCoroutine(finishRoutine);
        leftRoutine = rightRoutine = finishRoutine = null;

        // asegurarse de que las garras se destruyen
        boss.DestroyClaws();
    }
}
}
