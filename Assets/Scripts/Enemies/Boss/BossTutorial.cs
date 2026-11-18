using System.Collections;
using UnityEngine;
using Level;

namespace Enemies
{

/// <summary>
/// Jefe del tutorial. Se mueve por transform (barrido lateral y salto) y hace daño por contacto.
/// Sus movimientos quedan limitados por las puertas de su sala (lista doors).
/// </summary>
public class BossTutorial : BaseBoss
{
    [Header("Sala")]
    [Tooltip("Sala (GodRoom) del jefe: aparece, salta y hace sus embestidas siempre dentro de su rectángulo")]
    [SerializeField] private GodRoom godRoom;

    [Header("Ataques del Tutorial")]
    [Tooltip("Altura a la que sube el jefe en el ataque de salto (unidades de mundo)")]
    public float jumpHeight = 5f;
    [Tooltip("Separación extra con los bordes de la sala")]
    public float sideMargin = 0.1f;
    [Tooltip("Daño que se hace el propio jefe si completa un barrido sin golpear al jugador")]
    public float missDamage = 1f;
    [Tooltip("Segundos de invulnerabilidad del jugador tras un golpe del jefe")]
    public float hitCooldown = 0.3f;
    [Tooltip("Distancia máxima entre colliders para contar como contacto (unidades de mundo)")]
    public float contactTolerance = 0.05f;

    private bool hitPlayerThisSweep;
    private float nextHitTime;

    protected override void Awake()
    {
        health = 9f;
        maxHealth = 12f;
        damage = 1f;
        startCooldown = 3f;
        minCooldown = 3f;
        maxCooldown = 3f;
        availableStates = new System.Collections.Generic.List<string> { "sideAttack" };
        bossName = "tutorial";

        base.Awake();

        // NOTA: setScaleAndBody(2, 0.45, 0.4, 0, 0.05) de Phaser: ajusta escala y BoxCollider2D
        // directamente en el prefab (Scale 2, collider ~45% x 40% del sprite).

        SetupStates();
    }

    private void SetupStates()
    {
        AddState("sideAttack", new BossTutorialSideAttackState());
        AddState("jumpAttack", new BossTutorialJumpAttackState());
        AddState("cooldown", new BossTutorialCooldownState());
    }

    /// <summary>
    /// mitad del tamaño del cuerpo y desplazamiento del collider, en unidades de mundo
    /// (se calcula a partir del tamaño del BoxCollider2D, así funciona aunque esté desactivado)
    /// </summary>
    private void GetBodyMetrics(out Vector2 half, out Vector2 offset)
    {
        if (bodyCollider != null)
        {
            Vector2 scale = transform.lossyScale;
            half = new Vector2(bodyCollider.size.x * Mathf.Abs(scale.x), bodyCollider.size.y * Mathf.Abs(scale.y)) / 2f;
            offset = new Vector2(bodyCollider.offset.x * scale.x, bodyCollider.offset.y * scale.y);
        }
        else
        {
            half = new Vector2(0.5f, 0.5f);
            offset = Vector2.zero;
        }
    }

    /// <summary>
    /// ¿tiene sala asignada? (si no, usa las puertas / la cámara como antes)
    /// </summary>
    public bool HasRoom => godRoom != null && godRoom.HasArea;

    /// <summary>
    /// límites (para transform.position) dentro de los que el jefe tiene el cuerpo entero dentro de su sala
    /// </summary>
    private void GetPositionLimits(out Vector2 min, out Vector2 max)
    {
        GetBodyMetrics(out Vector2 half, out Vector2 offset);
        Bounds b = godRoom.RoomBounds;

        min = new Vector2(b.min.x + half.x + sideMargin, b.min.y + half.y + sideMargin) - offset;
        max = new Vector2(b.max.x - half.x - sideMargin, b.max.y - half.y - sideMargin) - offset;

        // si la sala es más pequeña que el jefe, se queda en el centro
        if (min.x > max.x) min.x = max.x = (min.x + max.x) / 2f;
        if (min.y > max.y) min.y = max.y = (min.y + max.y) / 2f;
    }


    /// <summary>
    /// límites horizontales (para transform.position) del movimiento del jefe:
    /// el rectángulo de su GodRoom; si no tiene sala, las puertas de la lista doors;
    /// y si tampoco hay, los bordes de la cámara.
    /// El jefe se mueve por transform, así que sin esto atravesaría paredes y puertas.
    /// </summary>
    public void GetHorizontalLimits(out float left, out float right)
    {
        if (HasRoom)
        {
            GetPositionLimits(out Vector2 min, out Vector2 max);
            left = min.x;
            right = max.x;
            return;
        }

        BossFx.GetCameraView(transform.position, out Vector2 center, out float halfW, out float halfH);
        left = center.x - halfW;
        right = center.x + halfW;

        float x = transform.position.x;
        float doorLeft = float.NegativeInfinity;
        float doorRight = float.PositiveInfinity;

        foreach (var door in doors)
        {
            if (door == null || !door.gameObject.activeInHierarchy) continue;

            foreach (var col in door.GetComponentsInChildren<Collider2D>())
            {
                if (!col.enabled) continue;
                Bounds b = col.bounds;

                if (b.center.x < x) doorLeft = Mathf.Max(doorLeft, b.max.x);   // puerta a la izquierda: su cara interior es el máximo
                else doorRight = Mathf.Min(doorRight, b.min.x);                // puerta a la derecha: su cara interior es el mínimo
            }
        }

        if (!float.IsNegativeInfinity(doorLeft)) left = doorLeft;
        if (!float.IsPositiveInfinity(doorRight)) right = doorRight;

        GetBodyMetrics(out Vector2 half, out Vector2 offset);
        left += half.x + sideMargin;
        right -= half.x + sideMargin;

        if (left > right) left = right = (left + right) / 2f;
    }

    /// <summary>
    /// altura máxima (para transform.position.y) a la que puede subir sin salirse de la sala;
    /// sin sala devuelve la altura pedida sin limitar
    /// </summary>
    public float ClampHeight(float desiredY)
    {
        if (!HasRoom) return desiredY;

        GetPositionLimits(out Vector2 min, out Vector2 max);
        return Mathf.Clamp(desiredY, min.y, max.y);
    }

    /// <summary>
    /// mete al jefe dentro del rectángulo de su sala (para cuando aparece o reaparece)
    /// </summary>
    public void KeepInsideRoom()
    {
        if (!HasRoom) return;

        GetPositionLimits(out Vector2 min, out Vector2 max);
        Vector3 p = transform.position;
        transform.position = new Vector3(Mathf.Clamp(p.x, min.x, max.x), Mathf.Clamp(p.y, min.y, max.y), p.z);
    }

    /// <summary>
    /// reproduce la intro del jefe Tutorial (no tiene: empieza directamente)
    /// </summary>
    public override void PlayIntro()
    {
        KeepInsideRoom();                       // aparece dentro de su sala
        gameObject.SetActive(true);
        if (SpriteRenderer != null) SpriteRenderer.enabled = true;
        SetLife();
        // TODO: emitir evento 'bossIntroFinished'
    }

    protected override void Update()
    {
        base.Update();

        // daño por contacto (equivalente al physics.add.overlap boss-jugador de Phaser)
        if (isActivated && notdead) TryHitPlayer();
    }

    /// <summary>
    /// golpea al jugador si está tocando al jefe y ha pasado el tiempo de espera entre golpes
    /// </summary>
    public void TryHitPlayer()
    {
        var p = Player;
        if (p == null || p.dead || p.PlayerCollider == null) return;
        if (bodyCollider == null || !bodyCollider.enabled) return;
        if (Time.time < nextHitTime) return;

        // contacto con una pequeña tolerancia: si el collider del jefe es sólido, el jugador se queda
        // a una distancia mínima de él (nunca se solapan), y un simple "solapan o no" fallaba con el jefe quieto
        ColliderDistance2D d = bodyCollider.Distance(p.PlayerCollider);
        if (!d.isValid || d.distance > contactTolerance) return;

        hitPlayerThisSweep = true;
        nextHitTime = Time.time + hitCooldown;

        int dir = p.transform.position.x < transform.position.x ? -1 : 1;
        p.TakeDamage(damage, dir);
    }

    public void ResetHitFlag() => hitPlayerThisSweep = false;
    public bool HitPlayerThisSweep => hitPlayerThisSweep;

    public override Color GetDamageTintColor() => new Color(1f, 0f, 0f);

    /// <summary>
    /// avanza a la siguiente fase del jefe Tutorial
    /// </summary>
    public override void NextPhase()
    {
        if (phase == 1)
        {
            phase = 2;
            health = maxHealth + 3f;
            availableStates.Add("jumpAttack");
            HandlePhaseTransition();
        }
        else
        {
            Die();
        }
    }

    /// <summary>
    /// el jefe desaparece 2 segundos y vuelve a aparecer, sin transición
    /// </summary>
    private void HandlePhaseTransition()
    {
        CleanupAllWarnings();
        DestroyAllAttackObjects();
        stateMachine?.SetState("inactive");
        hitPlayerThisSweep = false;

        // el GameObject se queda activo (solo se oculta): con un objeto inactivo la corrutina
        // no podría arrancar y el jefe no volvería nunca
        if (SpriteRenderer != null) SpriteRenderer.enabled = false;
        if (bodyCollider != null) bodyCollider.enabled = false;
        isActivated = false;

        StartCoroutine(PhaseTransitionRoutine());
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        yield return new WaitForSeconds(2f);

        KeepInsideRoom();                       // reaparece dentro de su sala
        if (SpriteRenderer != null) SpriteRenderer.enabled = true;
        if (bodyCollider != null) bodyCollider.enabled = true;

        isActivated = true;
        GenerateNewCooldown();
        stateMachine.SetState("cooldown");
    }

    /// <summary>
    /// muerte del jefe Tutorial: primero se desactiva el collider para que no siga haciendo daño
    /// </summary>
    public override void Die()
    {
        if (bodyCollider != null) bodyCollider.enabled = false;
        base.Die();
    }
}
}
