using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Estado de ataque de puño vertical para romper plataformas (jefe Ira).
/// </summary>
public class BossAngryPunchPlatformState : BaseBossAttackState
{
    private float spawnX;

    public BossAngryPunchPlatformState() : base(new BossAttackConfig
    {
        AttackName = "Puño Vertical",
        Phases = new List<string> { "warning", "attack", "cooldown" },
        WarningDuration = 1.2f,
        AttackDuration = 0.5f,
        CooldownDuration = 0.5f
    })
    { }

    /// <summary>
    /// crea la advertencia visual para el puño vertical, en la posición del jugador
    /// </summary>
    protected override void CreateWarning()
    {
        Camera cam = Camera.main;
        float camHeight = cam != null ? cam.orthographicSize * 2f : 20f;

        spawnX = player.transform.position.x;
        float warningWidth = 1.2f;

        CreateWarningRectangle(spawnX, boss.transform.position.y, warningWidth, camHeight, Color.red, 0.5f);
    }

    protected override void ExecuteAttack()
    {
        SpawnPunch();
    }

    private void SpawnPunch()
    {
        var angry = (BossAngry)boss;
        if (angry.PunchPrefab == null) return;

        Vector3 spawnPos = new Vector3(spawnX, boss.transform.position.y + 3f, 0f);
        var punch = Object.Instantiate(angry.PunchPrefab, spawnPos, Quaternion.identity);

        var attackObj = punch.GetComponent<BossAttackObject>();
        if (attackObj == null) attackObj = punch.AddComponent<BossAttackObject>();
        attackObj.isPlatformPunch = true;         // marcar como puño de plataforma

        boss.AddAttack(punch);

        var rb = punch.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0f, -angry.punchYSpeed);
        }

        SetupTimedCleanup(punch, 3f);
    }
}
}
