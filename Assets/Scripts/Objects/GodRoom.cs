using System.Collections;
using UnityEngine;
using Enemies;
using player;

namespace Level
{

/// <summary>
/// Sala de cámara fija para los combates contra jefes.
/// Mientras está activa, la cámara deja de seguir al jugador y se queda estática encuadrando
/// solo esta sala (con sus medidas). Se activa desde el InvisibleTrigger y se libera sola
/// cuando el jefe muere: entonces la cámara vuelve a seguir al jugador.
///
/// Uso: pon este script en un objeto vacío, añádele un BoxCollider2D que cubra exactamente la sala
/// (marca "Is Trigger" y déjalo sin tocar el layer para que no choque con nada), asígnalo en
/// "Room Area" y arrastra el GodRoom al campo "God Room" del InvisibleTrigger.
/// </summary>
public class GodRoom : MonoBehaviour
{
    [Header("Jefe")]
    [Tooltip("Jefe de esta sala (cualquiera: Tutorial, Fear, Angry, Sad, Final...). La cámara se libera cuando muere. " +
             "Si se deja vacío, se usa el jefe que le pase el InvisibleTrigger")]
    [SerializeField] private BaseBoss boss;

    [Header("Sala")]
    [Tooltip("BoxCollider2D que marca las medidas de la sala (lo que verá la cámara)")]
    [SerializeField] private BoxCollider2D roomArea;

    [Header("Cámara")]
    [Tooltip("Script que hace que la cámara siga al jugador (se desactiva mientras la sala está activa)")]
    [SerializeField] private CameraFollow cameraFollow;
    [Tooltip("Cámara a controlar. Si se deja vacía se usa Camera.main")]
    [SerializeField] private Camera cam;
    [Tooltip("Segundos que tarda la cámara en encuadrar la sala (y en volver al jugador)")]
    [Min(0f)] [SerializeField] private float transitionTime = 0.6f;
    [Tooltip("Margen extra alrededor de la sala, en unidades de mundo")]
    [SerializeField] private float padding = 0f;

    private bool active;
    private float previousSize;
    private Coroutine routine;

    public bool IsActive => active;

    /// <summary>
    /// rectángulo de la sala en coordenadas de mundo (lo usan los jefes para no salirse de ella)
    /// </summary>
    public Bounds RoomBounds => roomArea != null ? roomArea.bounds : new Bounds(transform.position, Vector3.zero);
    public bool HasArea => roomArea != null;

    /// <summary>
    /// activa la sala: la cámara se fija en ella hasta que muera el jefe de la sala
    /// (el elegido en el Inspector; si no hay, el que se pase por parámetro)
    /// </summary>
    public void Activate(BaseBoss roomBoss = null)
    {
        if (active) return;

        if (boss == null) boss = roomBoss;
        if (boss == null)
        {
            Debug.LogWarning("[GodRoom] No hay jefe asignado: elige uno en el Inspector o pásaselo desde el trigger");
            return;
        }

        if (cam == null) cam = Camera.main;
        if (cam == null || roomArea == null)
        {
            Debug.LogWarning("[GodRoom] Falta la cámara o el Room Area, no se puede fijar la cámara");
            return;
        }
        if (!cam.orthographic)
        {
            Debug.LogWarning("[GodRoom] La cámara no es ortográfica, no se puede encuadrar la sala");
            return;
        }

        active = true;
        previousSize = cam.orthographicSize;

        // el script de seguimiento se apaga: ahora manda la sala
        if (cameraFollow != null) cameraFollow.enabled = false;

        // tamaño ortográfico necesario para que quepa la sala entera (ancho y alto)
        Bounds b = roomArea.bounds;
        float halfH = b.extents.y + padding;
        float halfW = b.extents.x + padding;
        float targetSize = Mathf.Max(halfH, halfW / cam.aspect);
        Vector3 targetPos = new Vector3(b.center.x, b.center.y, cam.transform.position.z);

        StartRoutine(MoveCamera(targetPos, targetSize, false));
    }

    private void Update()
    {
        if (!active) return;

        // el jefe muere (Die() pone notdead a false) o desaparece: se libera la cámara
        if (boss == null || !boss.notdead) Release();
    }

    /// <summary>
    /// desactiva la sala: la cámara vuelve a seguir al jugador
    /// </summary>
    public void Release()
    {
        if (!active) return;
        active = false;

        Vector3 target = cam.transform.position;
        if (Player.Instance != null)
        {
            target = new Vector3(Player.Instance.transform.position.x, Player.Instance.transform.position.y, cam.transform.position.z);
        }

        StartRoutine(MoveCamera(target, previousSize, true));
    }

    private void StartRoutine(IEnumerator r)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(r);
    }

    private IEnumerator MoveCamera(Vector3 targetPos, float targetSize, bool returnToFollow)
    {
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        float t = 0f;
        while (t < transitionTime)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / transitionTime);
            cam.transform.position = Vector3.Lerp(startPos, targetPos, f);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, f);
            yield return null;
        }

        cam.transform.position = targetPos;
        cam.orthographicSize = targetSize;
        routine = null;

        // al terminar de volver, la cámara vuelve a seguir al jugador
        if (returnToFollow && cameraFollow != null) cameraFollow.enabled = true;
    }

    // la sala se ve en la Scene view aunque no esté seleccionada
    private void OnDrawGizmos()
    {
        if (roomArea == null) return;

        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
        Bounds b = roomArea.bounds;
        Gizmos.DrawWireCube(b.center, new Vector3(b.size.x + padding * 2f, b.size.y + padding * 2f, 0f));
    }
}
}
