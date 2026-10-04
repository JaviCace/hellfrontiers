using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Datos propios de la bola de agua (sustituye a las propiedades dinámicas
/// que Phaser le añadía al sprite: following, speed, health...).
/// </summary>
public class WaterBallData : MonoBehaviour
{
    public bool following = false;
    public float speed = 200f;
    public float health = 1f;
    public float maxHealth = 1f;
}

/// <summary>
/// Estado de ataque de bola de agua perseguidora para el jefe Tristeza.
/// </summary>
public class BossSadWaterBallState : BaseBossAttackState
{
    // fases con duración fija propia (no usan warning/attack/cooldownDuration del config)
    private const float SpawnPhaseDuration = 0.5f;
    private const float FollowPhaseDuration = 5f;
    private const float ExplodePhaseDuration = 0.6f;
    private const float CooldownPhaseDuration = 0.5f;

    private bool damageApplied;
    private bool ballDestroyed;
    private GameObject waterBall;
    private WaterBallData waterBallData;
    private float waterBallHealth = 1f;
    private Vector3 explosionPos;

    public BossSadWaterBallState() : base(new BossAttackConfig
    {
        AttackName = "Bola de Agua Perseguidora",
        Phases = new List<string> { "spawn", "follow", "explode", "cooldown" },
        WarningDuration = 0f,
        AttackDuration = 6f,
        CooldownDuration = 0.5f
    })
    { }

    public override void Enter(BaseBoss context, object data = null)
    {
        boss = context;
        player = boss.Player;
        currentPhase = "spawn";
        stateTime = 0f;

        StartSpawnPhase();
    }

    public override void Execute(BaseBoss context, float time, float delta)
    {
        stateTime += delta;

        switch (currentPhase)
        {
            case "spawn":
                if (stateTime >= SpawnPhaseDuration) StartFollowPhase();
                break;

            case "follow":
                FollowPlayer();
                CheckPlayerCollision();
                CheckAttackCollision();

                if (stateTime >= FollowPhaseDuration && !ballDestroyed) StartExplodePhase();
                break;

            case "explode":
                if (stateTime >= ExplodePhaseDuration) StartCooldownPhaseInternal();
                break;

            case "cooldown":
                if (stateTime >= CooldownPhaseDuration) boss.SelectNextState();
                break;
        }
    }

    protected override void CreateWarning() { }   // este ataque no tiene fase de warning
    protected override void ExecuteAttack() { }    // el ataque se maneja en las fases específicas

    private void StartSpawnPhase()
    {
        currentPhase = "spawn";
        stateTime = 0f;
        SpawnWaterBall();
    }

    /// <summary>
    /// genera la bola de agua
    /// </summary>
    private void SpawnWaterBall()
    {
        var sad = (BossSad)boss;
        if (sad.WaterBallPrefab == null) return;

        Vector3 spawnPos = boss.transform.position + new Vector3(0f, 0.5f, 0f); // "-50" en Phaser (arriba del boss)
        waterBall = Object.Instantiate(sad.WaterBallPrefab, spawnPos, Quaternion.identity);
        boss.AddAttack(waterBall); // añade también BossAttackObject (isProjectile = true por defecto)

        waterBallData = waterBall.GetComponent<WaterBallData>();
        if (waterBallData == null) waterBallData = waterBall.AddComponent<WaterBallData>();
        waterBallData.following = false;
        waterBallData.speed = sad.waterBallSpeed;
        waterBallData.health = waterBallHealth;
        waterBallData.maxHealth = waterBallHealth;

        // efecto de aparición (bounce)
        var sr = waterBall.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color; c.a = 0f; sr.color = c;
        }
        waterBall.transform.localScale = Vector3.one * 0.1f;
        boss.StartCoroutine(SpawnBounceRoutine(waterBall, sr, 0.4f));
    }

    private IEnumerator SpawnBounceRoutine(GameObject go, SpriteRenderer sr, float duration)
    {
        float t = 0f;
        while (t < duration && go != null)
        {
            t += Time.deltaTime;
            float f = EaseOutBack(t / duration);
            go.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 1.4f, f);
            if (sr != null)
            {
                var c = sr.color; c.a = Mathf.Clamp01(f); sr.color = c;
            }
            yield return null;
        }
    }

    private static float EaseOutBack(float t)
    {
        const float s = 1.70158f;
        float t2 = t - 1f;
        return t2 * t2 * ((s + 1f) * t2 + s) + 1f;
    }

    /// <summary>
    /// verifica colisión con ataques del jugador (melee; el ataque a distancia del jugador
    /// depende de tu ProjectilePool, revisa la nota TODO más abajo)
    /// </summary>
    private void CheckAttackCollision()
    {
        if (waterBall == null || !waterBall.activeInHierarchy || ballDestroyed) return;

        if (player.isAttacking)
        {
            boss.SafeDelay(0.05f, () =>
            {
                if (waterBall == null || !waterBall.activeInHierarchy || ballDestroyed) return;

                float attackDistance = player.meleeAttackDist * player.attackRangeMultiplier;
                float distanceToPlayer = Vector3.Distance(waterBall.transform.position, player.transform.position);

                if (distanceToPlayer <= attackDistance + 0.5f)
                {
                    TakeDamage(player.damage * player.damageMultiplier);
                }
            });
        }

        // TODO: comprobar colisión con los proyectiles a distancia del jugador.
        // Necesitas exponer la lista de proyectiles activos desde tu ProjectilePool
        // (en Phaser era scene.playerProjectilePool.projectiles.getChildren()).
    }

    /// <summary>
    /// aplica daño a la bola de agua
    /// </summary>
    private void TakeDamage(float dmg)
    {
        if (waterBall == null || !waterBall.activeInHierarchy || ballDestroyed || waterBallData == null) return;

        waterBallData.health -= dmg;
        ShowDamageEffect();

        if (waterBallData.health <= 0f) DestroyWaterBall(false);
    }

    /// <summary>
    /// muestra efecto visual de daño en la bola de agua
    /// </summary>
    private void ShowDamageEffect()
    {
        if (waterBall == null || !waterBall.activeInHierarchy) return;

        var sr = waterBall.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            boss.StartCoroutine(FlickerRoutine(sr));
        }

        SpawnParticleBurst(waterBall.transform.position, 5, new Color(0.53f, 0.81f, 0.92f), 0.8f,
            minSize: 0.1f, maxSize: 0.16f, minDistance: 0.5f, maxDistance: 1.5f, duration: 0.4f);
    }

    private IEnumerator FlickerRoutine(SpriteRenderer sr)
    {
        Color original = sr.color;
        for (int i = 0; i < 3; i++)
        {
            yield return FadeSpriteAlpha(sr, original.a, 0.5f, 0.1f);
            yield return FadeSpriteAlpha(sr, 0.5f, original.a, 0.1f);
        }
    }

    private IEnumerator FadeSpriteAlpha(SpriteRenderer sr, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration && sr != null)
        {
            t += Time.deltaTime;
            var c = sr.color;
            c.a = Mathf.Lerp(from, to, t / duration);
            sr.color = c;
            yield return null;
        }
    }

    /// <summary>
    /// verifica colisión directa con el jugador
    /// </summary>
    private void CheckPlayerCollision()
    {
        if (waterBall == null || !waterBall.activeInHierarchy || damageApplied || ballDestroyed) return;

        boss.SafeDelay(0.1f, () =>
        {
            if (waterBall == null || !waterBall.activeInHierarchy || damageApplied || ballDestroyed) return;

            float distance = Vector3.Distance(waterBall.transform.position, player.transform.position);

            if (distance <= 0.4f)
            {
                DestroyWaterBall(true, "player");
            }
        });
    }

    /// <summary>
    /// crea efecto visual de impacto (cuando la bola explota sobre el jugador)
    /// </summary>
    private void CreateImpactEffect()
    {
        var impactCircle = CreateColorCircleObject("ImpactCircle", 0.6f, new Color(0f, 0.75f, 1f), 0.7f);
        impactCircle.transform.position = explosionPos;
        boss.SafeDelay(0.3f, () => { if (impactCircle != null) Object.Destroy(impactCircle); });

        for (int i = 0; i < 3; i++)
        {
            SpawnExpandingRing(explosionPos, 0.3f, new Color(0.25f, 0.41f, 0.88f), 0.5f - i * 0.15f, 3f + i * 0.5f, 0.4f, i * 0.08f);
        }

        SpawnParticleBurst(explosionPos, 8, new Color(0.53f, 0.81f, 0.92f), 0.9f,
            minSize: 0.1f, maxSize: 0.1f, minDistance: 0.5f, maxDistance: 0.5f, duration: 0.3f, radial: true);

        // TODO: sacudida de cámara (shake(200, 0.015))
    }

    /// <summary>
    /// inicia la fase de explosión
    /// </summary>
    private void StartExplodePhase()
    {
        if (ballDestroyed || damageApplied)
        {
            StartCooldownPhaseInternal();
            return;
        }

        currentPhase = "explode";
        stateTime = 0f;

        if (waterBall != null && waterBall.activeInHierarchy)
        {
            DestroyWaterBall(false, "timeout");
        }
    }

    /// <summary>
    /// destruye la bola de agua con efecto visual (expansión + desvanecimiento)
    /// </summary>
    private void DestroyWaterBallWithEffect()
    {
        if (waterBall == null || !waterBall.activeInHierarchy) return;
        boss.StartCoroutine(DestroyWithEffectRoutine(waterBall));
    }

    private IEnumerator DestroyWithEffectRoutine(GameObject go)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        Vector3 startScale = go.transform.localScale;
        float t = 0f;
        while (t < 0.2f && go != null)
        {
            t += Time.deltaTime;
            float f = t / 0.2f;
            go.transform.localScale = Vector3.Lerp(startScale, startScale * 2f, f);
            if (sr != null) { var c = sr.color; c.a = Mathf.Lerp(1f, 0f, f); sr.color = c; }
            yield return null;
        }
        if (go != null) Object.Destroy(go);
    }

    /// <summary>
    /// destruye la bola de agua
    /// </summary>
    /// <param name="applyDamageToPlayer">si aplica daño al jugador (explotó sobre él)</param>
    /// <param name="damageSource">'player' (la destruyó el jugador) o 'timeout' (se acabó el tiempo)</param>
    private void DestroyWaterBall(bool applyDamageToPlayer = false, string damageSource = "player")
    {
        if (waterBall == null || !waterBall.activeInHierarchy || ballDestroyed) return;

        ballDestroyed = true;
        explosionPos = waterBall.transform.position;

        if (waterBallData != null && waterBallData.following)
        {
            waterBallData.following = false;
            var rb = waterBall.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        DestroyWaterBallWithEffect();

        if (applyDamageToPlayer && !damageApplied)
        {
            damageApplied = true;
            int dir = player.transform.position.x < explosionPos.x ? -1 : 1;
            player.TakeDamage(boss.damage, dir);
            CreateImpactEffect();
        }
        else
        {
            if (damageSource == "player")
                CreateExplosion(1f, 4f, new Color(1f, 0.27f, 0.27f), 12, 0.8f);
            else
                CreateExplosion(1.8f, 6f, new Color(0f, 0.75f, 1f), 20, 1.5f);
        }

        StartCooldownPhaseInternal();
    }

    /// <summary>
    /// inicia la fase de seguimiento al jugador
    /// </summary>
    private void StartFollowPhase()
    {
        currentPhase = "follow";
        stateTime = 0f;

        if (waterBallData != null) waterBallData.following = true;
        ballDestroyed = false;
    }

    private float trailTimer;

    /// <summary>
    /// hace que la bola de agua siga al jugador
    /// </summary>
    private void FollowPlayer()
    {
        if (waterBall == null || !waterBall.activeInHierarchy || waterBallData == null || !waterBallData.following || ballDestroyed) return;

        Vector3 dir = (player.transform.position - waterBall.transform.position).normalized;
        var rb = waterBall.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * waterBallData.speed;

        trailTimer += Time.deltaTime;
        if (trailTimer >= 0.3f)
        {
            trailTimer = 0f;
            CreateTrailEffect();
        }
    }

    /// <summary>
    /// crea efecto de rastro tras la bola de agua
    /// </summary>
    private void CreateTrailEffect()
    {
        if (waterBall == null || !waterBall.activeInHierarchy || ballDestroyed) return;

        var trail = CreateColorCircleObject("Trail", 0.15f, new Color(0.53f, 0.81f, 0.92f), 0.6f);
        trail.transform.position = waterBall.transform.position;
        boss.StartCoroutine(TrailFadeRoutine(trail));
    }

    private IEnumerator TrailFadeRoutine(GameObject trail)
    {
        var sr = trail.GetComponent<SpriteRenderer>();
        Vector3 startScale = trail.transform.localScale;
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float f = t / 0.3f;
            trail.transform.localScale = Vector3.Lerp(startScale, startScale * 0.5f, f);
            var c = sr.color; c.a = Mathf.Lerp(0.6f, 0f, f); sr.color = c;
            yield return null;
        }
        Object.Destroy(trail);
    }

    /// <summary>
    /// crea efecto visual de explosión
    /// </summary>
    private void CreateExplosion(float radius, float maxScale, Color tint, int particleCount, float maxDistance)
    {
        if (damageApplied) return;

        var sad = (BossSad)boss;
        GameObject explosionSprite;
        if (sad.WaterBallPrefab != null)
            explosionSprite = Object.Instantiate(sad.WaterBallPrefab, explosionPos, Quaternion.identity);
        else
            explosionSprite = CreateColorCircleObject("Explosion", 0.5f, tint, 0.8f);

        var explosionSr = explosionSprite.GetComponent<SpriteRenderer>();
        if (explosionSr != null) explosionSr.color = new Color(tint.r, tint.g, tint.b, 0.8f);
        explosionSprite.transform.localScale = Vector3.one * 0.5f;

        boss.StartCoroutine(ExplosionScaleRoutine(explosionSprite, explosionSr, maxScale, ExplodePhaseDuration));

        for (int waveNum = 0; waveNum < 4; waveNum++)
        {
            SpawnExpandingRing(explosionPos, radius * 0.3f, new Color(0.25f, 0.41f, 0.88f),
                0.4f - waveNum * 0.1f, 4f + waveNum * 0.3f, 0.5f, waveNum * 0.12f);
        }

        bool isRedExplosion = tint.r > 0.9f && tint.g < 0.4f;
        SpawnParticleBurst(explosionPos, particleCount,
            isRedExplosion ? new Color(1f, 0.27f, 0.27f) : new Color(0.53f, 0.81f, 0.92f), 0.9f,
            minSize: 0.12f, maxSize: 0.2f, minDistance: maxDistance * 0.6f, maxDistance: maxDistance, duration: 0.7f, radial: true);

        // daño por explosión (solo para la explosión normal azul, por timeout)
        if (!damageApplied && !isRedExplosion)
        {
            float distance = Vector3.Distance(explosionPos, player.transform.position);
            if (distance <= radius)
            {
                int dir = player.transform.position.x < explosionPos.x ? -1 : 1;
                player.TakeDamage(boss.damage, dir);
                damageApplied = true;
                // TODO: sacudida fuerte de cámara (shake(300, 0.02))
            }
        }
    }

    private IEnumerator ExplosionScaleRoutine(GameObject go, SpriteRenderer sr, float maxScale, float duration)
    {
        float t = 0f;
        while (t < duration && go != null)
        {
            t += Time.deltaTime;
            float f = 1f - Mathf.Pow(1f - t / duration, 3f); // ease-out cúbico
            go.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * maxScale, f);
            if (sr != null) { var c = sr.color; c.a = Mathf.Lerp(0.8f, 0f, f); sr.color = c; }
            yield return null;
        }
        if (go != null) Object.Destroy(go);
    }

    /// <summary>
    /// inicia la fase de cooldown
    /// </summary>
    private void StartCooldownPhaseInternal()
    {
        currentPhase = "cooldown";
        stateTime = 0f;
        damageApplied = false;
        ballDestroyed = false;
        waterBallHealth = 1f;
    }

    /// <summary>
    /// destruye todas las advertencias visuales (y la bola de agua, si sigue viva)
    /// </summary>
    public override void DestroyAllWarnings()
    {
        base.DestroyAllWarnings();

        if (waterBall != null && waterBall.activeInHierarchy)
        {
            Object.Destroy(waterBall);
            waterBall = null;
        }
    }

    public override void Exit(BaseBoss context)
    {
        DestroyAllWarnings();
        damageApplied = false;
        ballDestroyed = false;
        waterBallHealth = 1f;
    }
}
}
