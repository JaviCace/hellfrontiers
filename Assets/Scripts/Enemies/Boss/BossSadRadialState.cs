using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Estado de ataque radial de carámbanos para el jefe Tristeza.
/// </summary>
public class BossSadRadialState : BaseBossAttackState
{
    public BossSadRadialState() : base(new BossAttackConfig
    {
        AttackName = "Ataque Radial",
        Phases = new List<string> { "warning", "attack", "cooldown" },
        WarningDuration = 1.2f,
        AttackDuration = 0.9f,
        CooldownDuration = 1.5f
    })
    { }

    /// <summary>
    /// crea la advertencia circular alrededor del boss
    /// </summary>
    protected override void CreateWarning()
    {
        float radius = 1.5f; // eran 150px en Phaser, ajusta a tu escala

        var warningCircle = CreateWarningCircle(boss.transform.position.x, boss.transform.position.y, radius, new Color(0.25f, 0.41f, 0.88f), 0.3f);
        var warningBorder = CreateWarningCircleBorder(boss.transform.position.x, boss.transform.position.y, radius, new Color(0.53f, 0.81f, 0.92f));

        CreatePulseEffect(new List<WarningVisual> { warningCircle, warningBorder }, 0.6f, 0.5f, 0.8f);
    }

    protected override void ExecuteAttack()
    {
        SpawnRadialIcicles(12);
    }

    /// <summary>
    /// genera carámbanos en patrón radial
    /// </summary>
    private void SpawnRadialIcicles(int count)
    {
        var sad = (BossSad)boss;
        if (sad.IciclePrefab == null) return;

        float speed = sad.radialSpeed;
        float angleStep = Mathf.PI * 2f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            float velocityX = Mathf.Cos(angle) * speed;
            float velocityY = Mathf.Sin(angle) * speed;

            var icicle = Object.Instantiate(sad.IciclePrefab, boss.transform.position, Quaternion.identity);
            boss.AddAttack(icicle);

            var rb = icicle.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(velocityX, velocityY);
            }

            float rotationAngle = Mathf.Atan2(velocityY, velocityX) * Mathf.Rad2Deg;
            icicle.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

            SetupTimedCleanup(icicle, 3f);
        }
    }
}
}
