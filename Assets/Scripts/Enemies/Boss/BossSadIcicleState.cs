using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Estado de ataque de carámbanos verticales para el jefe Tristeza.
/// </summary>
public class BossSadIcicleState : BaseBossAttackState
{
    private float spawnX;

    public BossSadIcicleState() : base(new BossAttackConfig
    {
        AttackName = "Carámbanos Verticales",
        Phases = new List<string> { "warning", "attack", "cooldown" },
        WarningDuration = 1.2f,
        AttackDuration = 0.5f,
        CooldownDuration = 0.5f
    })
    { }

    /// <summary>
    /// crea la advertencia visual para los carámbanos verticales
    /// </summary>
    protected override void CreateWarning()
    {
        Camera cam = Camera.main;
        float camHeight = cam != null ? cam.orthographicSize * 2f : 20f;

        spawnX = player.transform.position.x;
        float warningWidth = 1.2f;

        CreateWarningRectangle(spawnX, boss.transform.position.y, warningWidth, camHeight, new Color(0.25f, 0.41f, 0.88f), 0.5f);
    }

    protected override void ExecuteAttack()
    {
        SpawnIcicle();
    }

    /// <summary>
    /// genera un carámbano vertical
    /// </summary>
    private void SpawnIcicle()
    {
        var sad = (BossSad)boss;
        if (sad.IciclePrefab == null) return;

        Vector3 spawnPos = new Vector3(spawnX, boss.transform.position.y + 4f, 0f);
        var icicle = Object.Instantiate(sad.IciclePrefab, spawnPos, Quaternion.Euler(0, 0, -90f)); // apuntando hacia abajo

        boss.AddAttack(icicle);

        var rb = icicle.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0f, -sad.icicleSpeed);
        }

        SetupTimedCleanup(icicle, 2f);
    }
}
}
