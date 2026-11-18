using System.Collections;
using UnityEngine;
using player;

namespace Level
{

/// <summary>
/// Lava que sube y mata al jugador si la toca (port de FloorIsLava.js).
///
/// Montaje en Unity:
///   - Objeto vacío "FloorIsLava" con este script (añade solo un Rigidbody2D kinemático y un BoxCollider2D trigger).
///   - Dentro, un hijo con un SpriteRenderer con la textura de lava -> arrástralo a "Visual".
///   - El Transform del objeto marca el BORDE SUPERIOR de la lava (como el origen (0.5, 0) de Phaser).
///   - Actívala llamando a StartLava() (por ejemplo desde un trigger) y páralas con StopLava().
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class FloorIsLava : MonoBehaviour
{
    [Header("Tamaño (unidades de mundo)")]
    [SerializeField] private float width = 20f;       // 2000 px
    [SerializeField] private float height = 5f;       // 500 px
    [Tooltip("El cuerpo físico es un poco más bajo que el dibujo (en Phaser, 20 px)")]
    [SerializeField] private float bodyShrink = 0.2f;

    [Header("Movimiento")]
    [Tooltip("Unidades de mundo por segundo (en Phaser: 20 px/s = 0.2)")]
    [SerializeField] private float riseSpeed = 0.2f;
    [Tooltip("Al empezar, la lava aparece esta distancia por debajo del jugador (en Phaser: 300 px = 3)")]
    [SerializeField] private float startOffsetBelowPlayer = 3f;
    [Tooltip("Cuánto baja al pararla y cuánto tarda (en Phaser: 50 px, 1 s)")]
    [SerializeField] private float stopDropDistance = 0.5f;
    [SerializeField] private float stopDropDuration = 1f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer visual;
    [Tooltip("Opcional: Renderer cuyo material hace scroll (como el tilePosition de Phaser). Vale un Quad con textura en modo Repeat")]
    [SerializeField] private Renderer scrollRenderer;
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0f, 0.3f);

    [Header("Referencias")]
    [SerializeField] private Camera cam;

    public bool IsRising { get; private set; }

    private Rigidbody2D rb;
    private BoxCollider2D box;
    private Material scrollMaterial;
    private float topY;
    private Coroutine stopRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        float bodyHeight = Mathf.Max(0.01f, height - bodyShrink);
        box.size = new Vector2(width, bodyHeight);
        box.offset = new Vector2(0f, -bodyHeight * 0.5f);   // cuelga desde el borde superior

        if (cam == null) cam = Camera.main;
        if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();

        if (visual != null)
        {
            visual.drawMode = SpriteDrawMode.Tiled;
            visual.size = new Vector2(width, height);
            visual.transform.localPosition = new Vector3(0f, -height * 0.5f, 0f); // asume pivote centrado
        }

        if (scrollRenderer != null) scrollMaterial = scrollRenderer.material;

        topY = transform.position.y;
        SetActiveState(false);
    }

    private void SetActiveState(bool on)
    {
        if (visual != null) visual.enabled = on;
        if (scrollRenderer != null) scrollRenderer.enabled = on;
    }

    private void Update()
    {
        // scroll de la textura (si hay material)
        if (scrollMaterial != null)
        {
            Vector2 offset = scrollMaterial.mainTextureOffset + scrollSpeed * Time.deltaTime;
            scrollMaterial.mainTextureOffset = new Vector2(Mathf.Repeat(offset.x, 1f), Mathf.Repeat(offset.y, 1f));
        }
    }

    private void FixedUpdate()
    {
        // centrar horizontalmente con la cámara
        float x = cam != null ? cam.transform.position.x : rb.position.x;

        if (IsRising) topY += riseSpeed * Time.fixedDeltaTime;

        rb.MovePosition(new Vector2(x, topY));
    }

    private void OnTriggerStay2D(Collider2D other) => TryKill(other);
    private void OnTriggerEnter2D(Collider2D other) => TryKill(other);

    private void TryKill(Collider2D other)
    {
        if (!IsRising) return;                          // solo mata si está subiendo
        Player p = other.GetComponentInParent<Player>();
        if (p == null || p.dead) return;
        p.Die();
    }

    /// <summary>
    /// inicia la subida de la lava, por debajo del jugador
    /// </summary>
    public void StartLava()
    {
        if (stopRoutine != null) { StopCoroutine(stopRoutine); stopRoutine = null; }

        Player p = Player.Instance;
        if (p != null) topY = p.transform.position.y - startOffsetBelowPlayer;

        IsRising = true;
        SetActiveState(true);
        rb.position = new Vector2(rb.position.x, topY);
    }

    /// <summary>
    /// detiene la subida y baja un poco suavemente
    /// </summary>
    public void StopLava()
    {
        if (!IsRising) return;
        IsRising = false;
        stopRoutine = StartCoroutine(DropRoutine());
    }

    private IEnumerator DropRoutine()
    {
        float startY = topY;
        float endY = topY - stopDropDistance;
        float t = 0f;
        while (t < stopDropDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / stopDropDuration);
            k = 1f - (1f - k) * (1f - k);               // ease out (Power2)
            topY = Mathf.Lerp(startY, endY, k);
            yield return null;
        }
        topY = endY;
        stopRoutine = null;
    }
}
}
