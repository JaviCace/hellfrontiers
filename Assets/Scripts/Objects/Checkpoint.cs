using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Managers;
using player;

namespace Level
{

/// <summary>
/// Punto de guardado (port de Checkpoint.js).
/// - Primera interacción: se activa, cura al jugador y guarda posición, jefes, botones y orbes.
/// - Siguientes interacciones: cura y abre el menú para elegir los 2 orbes equipados.
/// Interacción: tocar el checkpoint con el dedo mientras el jugador está en contacto con él (sin botones).
/// En el editor también vale el clic del ratón.
///
/// Montaje: sprite con SpriteRenderer + un Collider2D en modo Trigger que cubra la zona de interacción.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private float pulseScale = 1.1f;       // 5 -> 5.5 en Phaser
    [SerializeField] private float pulseDuration = 0.2f;

    [Header("Guardado")]
    [Tooltip("Dónde reaparece el jugador respecto al checkpoint (en Phaser: 50 px por encima)")]
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.5f);

    [Header("Menú de orbes")]
    [SerializeField] private OrbSelectMenu orbMenu;

    [Header("Sonido (opcional)")]
    [SerializeField] private AudioSource activateSound;

    [Header("Texto")]
    [SerializeField] private string activatePrompt = "Tap to activate checkpoint";
    [SerializeField] private string inventoryPrompt = "Tap for orb inventory";
    [SerializeField] private float promptHeight = 2f;
    [SerializeField] private float promptCharacterSize = 0.05f;

    public bool IsActivated { get; private set; }

    private static event Action<Checkpoint> CheckpointActivated;

    private Collider2D zone;
    private bool playerNearby;
    private TextMesh prompt;
    private Vector3 baseScale;
    private Camera cam;

    private Vector2 RespawnPoint => (Vector2)transform.position + respawnOffset;

    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        zone = GetComponent<Collider2D>();
        baseScale = transform.localScale;
        cam = Camera.main;
        CreatePrompt();
    }

    private void OnEnable() => CheckpointActivated += OnOtherActivated;
    private void OnDisable()
    {
        CheckpointActivated -= OnOtherActivated;
        playerNearby = false;
    }

    private void Start()
    {
        // si es el último checkpoint guardado, empieza activado
        SetActivated(PlayerDataManager.IsCurrentRespawn(RespawnPoint));
        if (orbMenu == null) orbMenu = FindFirstObjectByType<OrbSelectMenu>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        if (!playerNearby || GameFlow.MenuOpen) return;

        if (TappedOnThis()) Interact();
    }

    private bool TappedOnThis()
    {
        if (spriteRenderer == null) return false;
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;
                if (HitsSprite(touch.position.ReadValue())) return true;
            }
        }

#if UNITY_EDITOR
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && HitsSprite(mouse.position.ReadValue())) return true;
#endif

        return false;
    }

    private bool HitsSprite(Vector2 screenPos)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        world.z = spriteRenderer.bounds.center.z;
        return spriteRenderer.bounds.Contains(world) || (zone != null && zone.OverlapPoint(world));
    }

    // ---------------- Interacción ----------------

    /// <summary>
    /// activa el checkpoint, o si ya está activo abre el menú de orbes (también se puede llamar desde un botón de la UI)
    /// </summary>
    public void Interact()
    {
        Player p = Player.Instance;
        if (p == null || p.dead) return;

        if (IsActivated)
        {
            p.health = p.maxHealth;
            if (orbMenu != null) orbMenu.Open(p);
            else Debug.LogWarning("[Checkpoint] No hay OrbSelectMenu en la escena");
        }
        else
        {
            Activate(p);
        }
    }

    private void Activate(Player p)
    {
        if (IsActivated) return;

        SetActivated(true);
        CheckpointActivated?.Invoke(this);          // desactiva los demás
        if (activateSound != null) activateSound.Play();
        StartCoroutine(Pulse());

        Debug.Log("CHECKPOINT ACTIVADO en: " + transform.position);

        // curar y guardar
        p.health = p.maxHealth;
        PlayerDataManager.SaveCheckpoint(p, RespawnPoint);
    }

    private void OnOtherActivated(Checkpoint other)
    {
        if (other != this) SetActivated(false);
    }

    private void SetActivated(bool value)
    {
        IsActivated = value;
        if (spriteRenderer != null)
        {
            Sprite s = value ? onSprite : offSprite;
            if (s != null) spriteRenderer.sprite = s;
        }
        UpdatePrompt();
    }

    private IEnumerator Pulse()
    {
        float half = pulseDuration;
        Vector3 big = baseScale * pulseScale;
        float t = 0f;
        while (t < half) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(baseScale, big, t / half); yield return null; }
        t = 0f;
        while (t < half) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(big, baseScale, t / half); yield return null; }
        transform.localScale = baseScale;
    }

    // ---------------- Jugador cerca ----------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == null) return;
        playerNearby = true;
        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == null) return;
        playerNearby = false;
        UpdatePrompt();
    }

    // ---------------- Texto flotante ----------------

    private void CreatePrompt()
    {
        var go = new GameObject("CheckpointPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, promptHeight, 0f);
        prompt = go.AddComponent<TextMesh>();
        prompt.fontSize = 48;
        prompt.characterSize = promptCharacterSize;
        prompt.anchor = TextAnchor.MiddleCenter;
        prompt.alignment = TextAlignment.Center;
        prompt.color = Color.white;
        // el texto hereda la escala del checkpoint: se compensa para que no salga gigante
        Vector3 ls = transform.localScale;
        go.transform.localScale = new Vector3(1f / Mathf.Max(0.0001f, ls.x), 1f / Mathf.Max(0.0001f, ls.y), 1f);
        go.SetActive(false);
    }

    private void UpdatePrompt()
    {
        if (prompt == null) return;
        prompt.text = IsActivated ? inventoryPrompt : activatePrompt;
        prompt.gameObject.SetActive(playerNearby);
    }
}
}
