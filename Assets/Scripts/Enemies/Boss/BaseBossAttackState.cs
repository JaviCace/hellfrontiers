using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State;
using player;

namespace Enemies
{

/// <summary>
/// Configuración de un estado de ataque de jefe (equivalente al objeto config de Phaser).
/// </summary>
public class BossAttackConfig
{
    public string Texture = "default";
    public string AttackName = "Attack";
    public List<string> Phases = new List<string> { "warning", "attack", "cooldown" };
    public float WarningDuration = 2f;     // segundos
    public float AttackDuration = 0.5f;    // segundos
    public float CooldownDuration = 0.5f;  // segundos
    public bool LogOnEnter = true;
}

/// <summary>
/// Estado base de ataque para todos los jefes.
/// </summary>
public class BaseBossAttackState : BaseState<BaseBoss>
{
    protected BossAttackConfig config;
    protected BaseBoss boss;
    protected Player player;
    protected float stateTime;
    protected string currentPhase;

    protected readonly Dictionary<string, WarningVisual> warningElements = new Dictionary<string, WarningVisual>();
    private readonly List<Coroutine> pulseCoroutines = new List<Coroutine>();

    public BaseBossAttackState(BossAttackConfig config = null)
    {
        this.config = config ?? new BossAttackConfig();
        currentPhase = this.config.Phases[0];
    }

    public override void Enter(BaseBoss context, object data = null)
    {
        boss = context;
        player = boss.Player;
        stateTime = 0f;
        currentPhase = config.Phases[0];

        if (currentPhase == "warning") StartWarningPhase();
        else if (currentPhase == "attack") StartAttackPhase();
    }

    public override void Execute(BaseBoss context, float time, float delta)
    {
        stateTime += delta;

        switch (currentPhase)
        {
            case "warning":
                if (stateTime >= config.WarningDuration) StartAttackPhase();
                break;

            case "attack":
                if (stateTime >= config.AttackDuration) StartCooldownPhase();
                break;

            case "cooldown":
                if (stateTime >= config.CooldownDuration) boss.SelectNextState();
                break;
        }
    }

    protected void StartWarningPhase()
    {
        currentPhase = "warning";
        stateTime = 0f;
        CreateWarning();
    }

    protected void StartAttackPhase()
    {
        currentPhase = "attack";
        stateTime = 0f;
        DestroyAllWarnings();
        ExecuteAttack();
    }

    protected void StartCooldownPhase()
    {
        currentPhase = "cooldown";
        stateTime = 0f;
    }

    /// <summary>método abstracto - implementar en clases hijas</summary>
    protected virtual void CreateWarning()
    {
        throw new NotImplementedException("CreateWarning() debe ser implementado por la clase hija");
    }

    /// <summary>método abstracto - implementar en clases hijas</summary>
    protected virtual void ExecuteAttack()
    {
        throw new NotImplementedException("ExecuteAttack() debe ser implementado por la clase hija");
    }

    /// <summary>
    /// destruye todas las advertencias visuales
    /// </summary>
    public virtual void DestroyAllWarnings()
    {
        foreach (var visual in warningElements.Values) visual.Destroy();
        warningElements.Clear();

        foreach (var c in pulseCoroutines)
        {
            if (c != null) boss.StopCoroutine(c);
        }
        pulseCoroutines.Clear();
    }

    protected void RegisterWarningElement(string name, WarningVisual visual) => warningElements[name] = visual;
    protected WarningVisual GetWarningElement(string name) => warningElements.TryGetValue(name, out var v) ? v : null;

    public override void Exit(BaseBoss context)
    {
        DestroyAllWarnings();
        stateTime = 0f;
    }

    // ==================== Helpers visuales (equivalentes a los de Phaser) ====================

    /// <summary>
    /// crea un efecto de pulso visual (alpha yoyo, repetición infinita) sobre uno o varios elementos
    /// </summary>
    protected void CreatePulseEffect(List<WarningVisual> targets, float duration = 0.3f, float alphaFrom = 0.5f, float alphaTo = 0.8f)
    {
        pulseCoroutines.Add(boss.StartCoroutine(PulseRoutine(targets, duration, alphaFrom, alphaTo)));
    }

    private IEnumerator PulseRoutine(List<WarningVisual> targets, float duration, float alphaFrom, float alphaTo)
    {
        while (true)
        {
            yield return LerpAlpha(targets, alphaFrom, alphaTo, duration);
            yield return LerpAlpha(targets, alphaTo, alphaFrom, duration);
        }
    }

    private IEnumerator LerpAlpha(List<WarningVisual> targets, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            foreach (var v in targets) v.SetAlpha?.Invoke(a);
            yield return null;
        }
        foreach (var v in targets) v.SetAlpha?.Invoke(to);
    }

    /// <summary>
    /// crea texto flotante como advertencia (usa TextMesh, no requiere ningún asset de fuente)
    /// </summary>
    protected WarningVisual CreateFloatingText(float x, float y, string text, Color? color = null)
    {
        var go = new GameObject("WarningText");
        go.transform.position = new Vector3(x, y, 0f);
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 48;
        tm.characterSize = 0.2f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color ?? new Color(1f, 0.27f, 0.27f);

        var visual = new WarningVisual(go, a =>
        {
            var c = tm.color;
            c.a = a;
            tm.color = c;
        });
        RegisterWarningElement("warningText", visual);
        return visual;
    }

    /// <summary>
    /// crea un rectángulo de advertencia (sprite procedural, no requiere assets)
    /// </summary>
    protected WarningVisual CreateWarningRectangle(float x, float y, float width, float height, Color color, float alpha = 0.3f)
    {
        var go = CreateColorSpriteObject("WarningRect", width, height, color, alpha);
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.GetComponent<SpriteRenderer>();
        var visual = new WarningVisual(go, a =>
        {
            var c = sr.color;
            c.a = a;
            sr.color = c;
        });
        RegisterWarningElement("warningRect", visual);
        return visual;
    }

    /// <summary>
    /// crea un círculo de advertencia (sprite procedural, no requiere assets)
    /// </summary>
    protected WarningVisual CreateWarningCircle(float x, float y, float radius, Color color, float alpha = 0.3f)
    {
        var go = CreateColorCircleObject("WarningCircle", radius, color, alpha);
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.GetComponent<SpriteRenderer>();
        var visual = new WarningVisual(go, a =>
        {
            var c = sr.color;
            c.a = a;
            sr.color = c;
        });
        RegisterWarningElement("warningCircle", visual);
        return visual;
    }

    /// <summary>
    /// crea un borde de advertencia (LineRenderer formando un rectángulo hueco)
    /// </summary>
    protected WarningVisual CreateWarningBorder(float x, float y, float width, float height, Color color, float lineWidth = 0.08f)
    {
        var go = new GameObject("WarningBorder");
        go.transform.position = Vector3.zero;
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;
        lr.positionCount = 4;
        float hw = width / 2f, hh = height / 2f;
        lr.SetPositions(new Vector3[]
        {
            new Vector3(x - hw, y - hh, 0f),
            new Vector3(x + hw, y - hh, 0f),
            new Vector3(x + hw, y + hh, 0f),
            new Vector3(x - hw, y + hh, 0f)
        });

        var visual = new WarningVisual(go, a =>
        {
            var c = lr.startColor;
            c.a = a;
            lr.startColor = lr.endColor = c;
        });
        RegisterWarningElement("warningBorder", visual);
        return visual;
    }

    /// <summary>
    /// crea un borde circular de advertencia (LineRenderer formando un círculo)
    /// </summary>
    protected WarningVisual CreateWarningCircleBorder(float x, float y, float radius, Color color, float lineWidth = 0.08f, int segments = 32)
    {
        var go = new GameObject("WarningCircleBorder");
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;
        lr.positionCount = segments;

        var points = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            points[i] = new Vector3(x + Mathf.Cos(angle) * radius, y + Mathf.Sin(angle) * radius, 0f);
        }
        lr.SetPositions(points);

        var visual = new WarningVisual(go, a =>
        {
            var c = lr.startColor;
            c.a = a;
            lr.startColor = lr.endColor = c;
        });
        RegisterWarningElement("warningBorder", visual);
        return visual;
    }

    /// <summary>
    /// configura la auto-destrucción por tiempo de un GameObject
    /// </summary>
    protected void SetupTimedCleanup(GameObject obj, float delay = 3f)
    {
        boss.SafeDelay(delay, () =>
        {
            if (obj != null) UnityEngine.Object.Destroy(obj);
        });
    }

    /// <summary>
    /// lanza una ráfaga de partículas circulares en direcciones aleatorias/radiales desde un punto,
    /// desvaneciéndose y encogiéndose hasta desaparecer (reutilizado por varios efectos de daño/explosión)
    /// </summary>
    protected void SpawnParticleBurst(Vector3 origin, int count, Color color, float alpha,
        float minSize = 0.1f, float maxSize = 0.2f, float minDistance = 0.5f, float maxDistance = 1f,
        float duration = 0.3f, bool radial = false)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = radial ? (i / (float)count) * Mathf.PI * 2f : UnityEngine.Random.value * Mathf.PI * 2f;
            float distance = Mathf.Lerp(minDistance, maxDistance, UnityEngine.Random.value);
            float size = Mathf.Lerp(minSize, maxSize, UnityEngine.Random.value);

            var particle = CreateColorCircleObject("Particle", size, color, alpha);
            particle.transform.position = origin;

            Vector3 target = origin + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
            boss.StartCoroutine(ParticleFlyRoutine(particle, origin, target, duration));
        }
    }

    private IEnumerator ParticleFlyRoutine(GameObject particle, Vector3 from, Vector3 to, float duration)
    {
        var sr = particle.GetComponent<SpriteRenderer>();
        float t = 0f;
        Color startColor = sr.color;
        Vector3 startScale = particle.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float f = t / duration;
            particle.transform.position = Vector3.Lerp(from, to, f);
            particle.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, f);
            var c = sr.color; c.a = Mathf.Lerp(startColor.a, 0f, f); sr.color = c;
            yield return null;
        }
        UnityEngine.Object.Destroy(particle);
    }

    /// <summary>
    /// lanza un anillo (círculo hueco) que se expande y desvanece, opcionalmente con retraso
    /// (reutilizado por los efectos de onda de impacto/explosión)
    /// </summary>
    protected void SpawnExpandingRing(Vector3 origin, float startRadius, Color color, float alpha,
        float targetScale, float duration, float delay = 0f)
    {
        boss.StartCoroutine(ExpandingRingRoutine(origin, startRadius, color, alpha, targetScale, duration, delay));
    }

    private IEnumerator ExpandingRingRoutine(Vector3 origin, float startRadius, Color color, float alpha,
        float targetScale, float duration, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        var ring = CreateColorCircleObject("Ring", startRadius, color, alpha);
        ring.transform.position = origin;
        var sr = ring.GetComponent<SpriteRenderer>();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float f = t / duration;
            ring.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * targetScale, f);
            var c = sr.color; c.a = Mathf.Lerp(alpha, 0f, f); sr.color = c;
            yield return null;
        }
        UnityEngine.Object.Destroy(ring);
    }

    // ==================== Utilidades procedurales (sin assets externos) ====================

    /// <summary>
    /// crea un GameObject con un sprite de color sólido del tamaño pedido, sin necesitar ningún asset
    /// </summary>
    public static GameObject CreateColorSpriteObject(string name, float width, float height, Color color, float alpha, int sortingOrder = 100)
    {
        var tex = new Texture2D(2, 2);
        var c = color; c.a = alpha;
        tex.SetPixels(new[] { c, c, c, c });
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);

        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        go.transform.localScale = new Vector3(width, height, 1f);

        return go;
    }

    /// <summary>
    /// crea un GameObject con un sprite circular de color sólido, sin necesitar ningún asset
    /// </summary>
    public static GameObject CreateColorCircleObject(string name, float radius, Color color, float alpha, int sortingOrder = 100)
    {
        int size = 64;
        var tex = new Texture2D(size, size);
        var pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                Color c = color;
                c.a = dist <= size / 2f ? alpha : 0f;
                pixels[y * size + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / (radius * 2f));

        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        return go;
    }
}

/// <summary>
/// Envoltorio para un elemento visual de advertencia, sea cual sea su tipo real
/// (SpriteRenderer, LineRenderer, TextMesh...), para poder destruirlo y variar su alpha de forma uniforme.
/// </summary>
public class WarningVisual
{
    public GameObject GameObject;
    public Action<float> SetAlpha;

    public WarningVisual(GameObject go, Action<float> setAlpha)
    {
        GameObject = go;
        SetAlpha = setAlpha;
    }

    public void Destroy()
    {
        if (GameObject != null) UnityEngine.Object.Destroy(GameObject);
    }
}
}
