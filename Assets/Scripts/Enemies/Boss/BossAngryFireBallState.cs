using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Estado de ataque de bolas de fuego para el jefe Ira (bullet hell).
/// Dispara, una tras otra, el número de bolas y con el intervalo que se eligen en el
/// Inspector del jefe (BossAngry). Cada bola sale con un ángulo aleatorio dentro del
/// rango elegido, siempre hacia abajo.
/// </summary>
public class BossAngryFireBallState : BaseBossAttackState
{
    private float timeSinceLastSpawn;
    private int firedCount;

    public BossAngryFireBallState() : base(new BossAttackConfig
    {
        AttackName = "Bolas de Fuego",
        Phases = new List<string> { "attack", "cooldown" }, // sin fase de warning
        AttackDuration = 6f,
        CooldownDuration = 0.5f
    })
    { }

    public override void Enter(BaseBoss context, object data = null)
    {
        base.Enter(context, data);
        firedCount = 0;

        // la primera bola sale al instante
        timeSinceLastSpawn = ((BossAngry)context).fireballInterval;
    }

    public override void Execute(BaseBoss context, float time, float delta)
    {
        stateTime += delta;
        timeSinceLastSpawn += delta;

        switch (currentPhase)
        {
            case "attack":
            {
                var angry = (BossAngry)boss;
                float interval = Mathf.Max(0.01f, angry.fireballInterval);

                // dispara todas las bolas que toquen en este frame (mantiene el ritmo aunque bajen los fps)
                while (firedCount < angry.fireballCount && timeSinceLastSpawn >= interval)
                {
                    FireBullet(angry);
                    firedCount++;
                    timeSinceLastSpawn -= interval;
                }

                // el ataque termina cuando se han disparado todas
                if (firedCount >= angry.fireballCount) StartCooldownPhase();
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

    /// <summary>el ataque se ejecuta continuamente durante la fase attack (lógica en Execute)</summary>
    protected override void ExecuteAttack() { }

    /// <summary>
    /// dispara una bola de fuego desde el jefe con un ángulo aleatorio hacia abajo
    /// </summary>
    private void FireBullet(BossAngry angry)
    {
        if (angry.FireballPrefab == null) return;

        // ángulo "de pantalla": 0° = derecha, 90° = recto hacia abajo, 180° = izquierda.
        // Se limita a 1°-179° para que la bola vaya siempre hacia abajo.
        float a = Mathf.Clamp(angry.fireballMinAngle, 1f, 179f);
        float b = Mathf.Clamp(angry.fireballMaxAngle, 1f, 179f);
        float angle = Random.Range(Mathf.Min(a, b), Mathf.Max(a, b));

        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad));

        Vector3 position = boss.transform.position + (Vector3)angry.fireballSpawnOffset;
        var fireball = Object.Instantiate(angry.FireballPrefab, position, Quaternion.identity);
        boss.AddAttack(fireball);

        var rb = fireball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * angry.fireballSpeed;
        }

        SetupTimedCleanup(fireball, GetLifetime(angry));
    }

    /// <summary>
    /// tiempo de vida suficiente para que la bola cruce toda la pantalla antes de destruirse
    /// </summary>
    private float GetLifetime(BossAngry angry)
    {
        Camera cam = Camera.main;
        float halfHeight = cam != null ? cam.orthographicSize : 6f;
        float halfWidth = cam != null ? halfHeight * cam.aspect : 10f;

        float screenDiagonal = 2f * Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
        return Mathf.Max(3f, (screenDiagonal + 2f) / Mathf.Max(0.1f, angry.fireballSpeed));
    }
}
}
