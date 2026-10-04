using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using player;

namespace Level
{

/// <summary>
/// Puerta del jefe, con lógica de teletransporte a su puerta contraria.
/// Se activa tocando la puerta (pantalla táctil, o ratón en el editor)
/// mientras el jugador está en contacto con ella.
/// </summary>
public class DoorBoss : Door
{
    [Header("Teletransporte")]
    [SerializeField] private DoorBoss contrary;
    public bool isOpening = false;

    private bool playerTouching;
    private Camera cam;

    protected override void Awake()
    {
        base.Awake();

        cam = Camera.main;
        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
    }

    private void OnDisable()
    {
        playerTouching = false;
    }

    private void Update()
    {
        if (!playerTouching || isOpening) return;

        var touchscreen = Touchscreen.current;
        if (touchscreen == null) return;

        if (cam == null) cam = Camera.main;
        if (cam == null || doorCollider == null) return;

        // se revisan todos los dedos, no solo el primero: así funciona
        // aunque otro dedo esté sujetando el joystick o un botón
        foreach (var touch in touchscreen.touches)
        {
            if (!touch.press.wasPressedThisFrame) continue;

            Vector2 screenPos = touch.position.ReadValue();
            Vector3 worldPos = cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));

            // solo si el toque cae encima de esta puerta
            if (doorCollider.OverlapPoint(worldPos))
            {
                OpenDoor();
                return;
            }
        }
    }

    // --- Detección de contacto con el jugador (sirve con collider trigger o sólido) ---

    private static bool IsPlayer(Collider2D other) => other.GetComponentInParent<Player>() != null;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other)) playerTouching = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other)) playerTouching = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayer(collision.collider)) playerTouching = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsPlayer(collision.collider)) playerTouching = false;
    }

    /// <summary>
    /// posición actual de la puerta
    /// </summary>
    public Vector3 GetPosition() => transform.position;

    /// <summary>
    /// vincula la puerta de destino
    /// </summary>
    public void SetContrary(DoorBoss contraryDoor) => contrary = contraryDoor;

    /// <summary>
    /// abre la puerta y teletransporta al jugador
    /// </summary>
    public override void OpenDoor()
    {
        if (contrary == null) return;
        if (isOpening) return;

        isOpening = true;

        var p = Player.Instance;
        if (p != null)
        {
            p.canMove = false;
            p.SetVelocity(0, 0);
            p.PlayAnim("Player_idle", true);
            p.invulnerable = true;
            // invulnerable hasta que termine el teletransporte (ver FinishTeleportAfterFadeIn)
        }

        StartCoroutine(FadeOutTP());
    }

    /// <summary>
    /// fundido a negro antes de teletransportar (TODO: conecta esto a tu propio sistema de fundido de cámara)
    /// </summary>
    private IEnumerator FadeOutTP()
    {
        // 1 s de fundido + 1 s de espera
        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(1f);
        DoTeleport();
    }

    /// <summary>
    /// ejecuta el teletransporte físico del jugador
    /// </summary>
    private void DoTeleport()
    {
        var p = Player.Instance;
        if (p == null) return;

        Vector3 destino = contrary.GetPosition();
        p.transform.position = new Vector3(destino.x, destino.y + 2.1f, destino.z); // ajusta el desplazamiento a tu escala

        // TODO: fundido de entrada (fade in) de la cámara
        StartCoroutine(FinishTeleportAfterFadeIn(p));
    }

    private IEnumerator FinishTeleportAfterFadeIn(Player player)
    {
        yield return new WaitForSeconds(1f);

        player.canMove = true;
        player.invulnerable = false;
        isOpening = false;
    }
}
}
