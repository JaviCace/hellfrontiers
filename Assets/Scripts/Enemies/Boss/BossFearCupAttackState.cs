using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Estado de ataque de tazas para el jefe Miedo (y el Final): caen tazas desde arriba de la pantalla.
/// Número, intervalo, velocidad y prefab se eligen en el Inspector del jefe (IFearBoss).
/// </summary>
public class BossFearCupAttackState : BaseBossAttackState
{
    private float timeSinceLastSpawn;
    private int cupsSpawned;

    public BossFearCupAttackState() : base(new BossAttackConfig
    {
        AttackName = "Ataque de Tazas",
        Phases = new List<string> { "attack", "cooldown" }, // sin fase de warning
        AttackDuration = 6f,
        CooldownDuration = 1.5f
    })
    { }

    public override void Enter(BaseBoss context, object data = null)
    {
        base.Enter(context, data);
        cupsSpawned = 0;
        timeSinceLastSpawn = 0f;
    }

    public override void Execute(BaseBoss context, float time, float delta)
    {
        stateTime += delta;

        switch (currentPhase)
        {
            case "attack":
            {
                var fear = (IFearBoss)boss;
                timeSinceLastSpawn += delta;

                if (cupsSpawned < fear.CupCount && timeSinceLastSpawn >= Mathf.Max(0.01f, fear.CupInterval))
                {
                    SpawnCup(fear);
                    timeSinceLastSpawn = 0f;
                    cupsSpawned++;
                }

                if (stateTime >= config.AttackDuration) StartCooldownPhase();
                break;
            }

            case "cooldown":
                if (stateTime >= config.CooldownDuration) boss.SelectNextState();
                break;
        }
    }

    /// <summary>este ataque no tiene fase de warning</summary>
    protected override void CreateWarning()
    {
        StartAttackPhase();
    }

    /// <summary>el spawn de tazas se hace en Execute durante la fase attack</summary>
    protected override void ExecuteAttack() { }

    /// <summary>
    /// genera una taza en una posición aleatoria por encima de la pantalla
    /// </summary>
    private void SpawnCup(IFearBoss fear)
    {
        if (fear.CupPrefab == null) return;

        BossFx.GetCameraView(boss.transform.position, out Vector2 center, out float halfW, out float halfH);

        // margen de 1 unidad a cada lado (eran 100 px en Phaser)
        float x = Random.Range(center.x - halfW + 1f, center.x + halfW - 1f);
        float y = center.y + halfH + 0.5f;

        var cup = Object.Instantiate(fear.CupPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        boss.AddAttack(cup);

        var rb = cup.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.down * fear.CupSpeed;
        }

        // efecto de aparición (fade in)
        var sr = cup.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color; c.a = 0f; sr.color = c;
            boss.StartCoroutine(FadeIn(sr, 0.2f));
        }

        // se destruye al salir por debajo de la pantalla
        float lifetime = (2f * halfH + 3f) / Mathf.Max(0.1f, fear.CupSpeed);
        SetupTimedCleanup(cup, lifetime);
    }

    private IEnumerator FadeIn(SpriteRenderer sr, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            if (sr == null) yield break;
            t += Time.deltaTime;
            var c = sr.color;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            sr.color = c;
            yield return null;
        }
    }
}
}
