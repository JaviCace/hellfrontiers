using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Estado de ataque de puño horizontal para el jefe Ira.
/// </summary>
public class BossAngryPunchState : BaseBossAttackState
{
    // NOTA: los tamaños/paddings de aquí abajo (120, 150, 40, 80...) eran píxeles de Phaser.
    // En Unity son unidades de mundo: ajústalos a la escala (Pixels Per Unit) de tu proyecto.

    private float fixedSpawnY;
    private string attackDirection;
    private float spawnY;
    private readonly List<GameObject> arrowObjects = new List<GameObject>();
    private readonly List<Coroutine> arrowCoroutines = new List<Coroutine>();

    public BossAngryPunchState() : base(new BossAttackConfig
    {
        AttackName = "Puño Horizontal",
        Phases = new List<string> { "warning", "attack", "cooldown" },
        WarningDuration = 2f,
        AttackDuration = 0.5f,
        CooldownDuration = 0.5f
    })
    { }

    public override void Enter(BaseBoss context, object data = null)
    {
        // calcular posición Y fija antes de llamar al padre
        var angry = (BossAngry)context;
        fixedSpawnY = context.transform.position.y - angry.distanceToFloor; // distanceToFloor ya está en unidades de mundo

        base.Enter(context, data);
    }

    /// <summary>
    /// crea las advertencias visuales para el puño horizontal
    /// </summary>
    protected override void CreateWarning()
    {
        Camera cam = Camera.main;
        float camHalfWidth = cam != null ? cam.orthographicSize * cam.aspect : 10f;
        float camWidth = camHalfWidth * 2f;

        // solo ataques laterales
        attackDirection = Random.Range(0, 2) == 0 ? "left" : "right";

        float warningHeight = 1.2f;
        spawnY = fixedSpawnY;

        var warningRect = CreateWarningRectangle(boss.transform.position.x, spawnY, camWidth + 1.5f, warningHeight, Color.red, 0.3f);
        var warningBorder = CreateWarningBorder(boss.transform.position.x, spawnY, camWidth + 1.5f, warningHeight, new Color(1f, 0.27f, 0.27f, 0.8f));

        CreateDirectionArrows();

        CreatePulseEffect(new List<WarningVisual> { warningRect, warningBorder }, 0.3f, 0.5f, 0.8f);

        var warningText = CreateFloatingText(boss.transform.position.x, spawnY - warningHeight / 2f - 0.3f, "¡PUÑO INMINENTE!", new Color(1f, 0.27f, 0.27f));
        CreatePulseEffect(new List<WarningVisual> { warningText }, 0.2f, 0.3f, 1f);
    }

    /// <summary>
    /// crea flechas direccionales para indicar la dirección del ataque
    /// </summary>
    private void CreateDirectionArrows()
    {
        Camera cam = Camera.main;
        float camHalfWidth = cam != null ? cam.orthographicSize * cam.aspect : 10f;

        float arrowSize = 0.4f;
        float arrowSpacing = 0.8f;
        int numArrows = 5;

        foreach (var c in arrowCoroutines) if (c != null) boss.StopCoroutine(c);
        arrowCoroutines.Clear();
        arrowObjects.Clear();

        for (int i = 0; i < numArrows; i++)
        {
            float arrowX, arrowY = spawnY;

            if (attackDirection == "left")
                arrowX = boss.transform.position.x + camHalfWidth - (i * arrowSpacing) - 1f;
            else
                arrowX = boss.transform.position.x - camHalfWidth + (i * arrowSpacing) + 1f;

            bool pointsLeft = attackDirection == "left"; // "left" ataca hacia la izquierda -> flechas "←"
            var arrow = CreateTriangleArrow(arrowX, arrowY, arrowSize, new Color(1f, 0.27f, 0.27f, 0.8f), pointsLeft);
            arrowObjects.Add(arrow);

            float targetX = attackDirection == "left" ? arrowX - 0.2f : arrowX + 0.2f;
            arrowCoroutines.Add(boss.StartCoroutine(OscillatePosition(arrow.transform, arrowX, targetX, arrowY, 0.3f, i * 0.05f)));
        }

        // flecha grande central
        var bigArrow = CreateBigDirectionArrow();
        arrowObjects.Add(bigArrow);
    }

    /// <summary>
    /// crea una flecha grande central para destacar la dirección
    /// </summary>
    private GameObject CreateBigDirectionArrow()
    {
        Camera cam = Camera.main;
        float camHalfWidth = cam != null ? cam.orthographicSize * cam.aspect : 10f;
        float bigArrowSize = 0.6f;

        float bigArrowX = attackDirection == "left" ? boss.transform.position.x + camHalfWidth - 0.5f : boss.transform.position.x - camHalfWidth + 0.5f;
        bool pointsLeft = attackDirection == "left";

        var bigArrow = CreateTriangleArrow(bigArrowX, spawnY, bigArrowSize, Color.red, pointsLeft);
        boss.StartCoroutine(PulseScale(bigArrow.transform, 1f, 1.2f, 0.4f));
        return bigArrow;
    }

    /// <summary>
    /// crea una flecha triangular procedural (sin necesitar ningún asset)
    /// </summary>
    private GameObject CreateTriangleArrow(float x, float y, float size, Color color, bool pointsLeft)
    {
        var go = new GameObject("DirectionArrow");
        go.transform.position = new Vector3(x, y, 0f);

        var mesh = new Mesh();
        float dir = pointsLeft ? -1f : 1f;
        mesh.vertices = new[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(dir * size, size / 2f, 0f),
            new Vector3(dir * size, -size / 2f, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = color;

        return go;
    }

    private IEnumerator OscillatePosition(Transform t, float fromX, float toX, float y, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        while (true)
        {
            yield return LerpPositionX(t, fromX, toX, y, duration);
            yield return LerpPositionX(t, toX, fromX, y, duration);
        }
    }

    private IEnumerator LerpPositionX(Transform t, float fromX, float toX, float y, float duration)
    {
        float e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            t.position = new Vector3(Mathf.Lerp(fromX, toX, e / duration), y, 0f);
            yield return null;
        }
    }

    private IEnumerator PulseScale(Transform t, float from, float to, float duration)
    {
        while (true)
        {
            yield return LerpScale(t, from, to, duration);
            yield return LerpScale(t, to, from, duration);
        }
    }

    private IEnumerator LerpScale(Transform t, float from, float to, float duration)
    {
        float e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            float s = Mathf.Lerp(from, to, e / duration);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }

    protected override void ExecuteAttack()
    {
        SpawnPunch();
    }

    /// <summary>
    /// genera un puño horizontal
    /// </summary>
    private void SpawnPunch()
    {
        var angry = (BossAngry)boss;
        if (angry.PunchPrefab == null) return;

        Camera cam = Camera.main;
        float camHalfWidth = cam != null ? cam.orthographicSize * cam.aspect : 10f;

        // "left" = el puño entra por la derecha y viaja hacia la izquierda (igual que las flechas del aviso)
        bool goesLeft = attackDirection == "left";
        float startX = goesLeft ? boss.transform.position.x + camHalfWidth : boss.transform.position.x - camHalfWidth;
        Vector3 spawnPos = new Vector3(startX, fixedSpawnY, 0f);

        var punch = Object.Instantiate(angry.PunchPrefab, spawnPos, Quaternion.Euler(0, 0, goesLeft ? 180f : 0f));

        var attackObj = punch.GetComponent<BossAttackObject>();
        if (attackObj == null) attackObj = punch.AddComponent<BossAttackObject>();
        attackObj.isPlatformPunch = false; // puños laterales NO son platformPunch

        boss.AddAttack(punch);

        var rb = punch.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            float xSpeed = goesLeft ? -angry.punchXSpeed : angry.punchXSpeed;
            rb.linearVelocity = new Vector2(xSpeed, 0f);
        }

        // efecto de aparición (fade in)
        var sr = punch.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color; c.a = 0f; sr.color = c;
            boss.StartCoroutine(FadeIn(sr, 0.1f));
        }

        SetupTimedCleanup(punch, 5f);
    }

    private IEnumerator FadeIn(SpriteRenderer sr, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            var c = sr.color;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            sr.color = c;
            yield return null;
        }
    }

    /// <summary>
    /// destruye todas las advertencias visuales, incluidas las flechas
    /// </summary>
    public override void DestroyAllWarnings()
    {
        base.DestroyAllWarnings();

        foreach (var c in arrowCoroutines) if (c != null) boss.StopCoroutine(c);
        arrowCoroutines.Clear();

        foreach (var arrow in arrowObjects) if (arrow != null) Object.Destroy(arrow);
        arrowObjects.Clear();
    }
}
}
