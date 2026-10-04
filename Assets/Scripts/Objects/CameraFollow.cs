using UnityEngine;
using player;

/// <summary>
/// Cámara que sigue al jugador en 2D con suavizado, y opcionalmente limitada a los
/// bordes del nivel. Pon este script en la cámara (normalmente Main Camera).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;              // si lo dejas vacío, usa Player.Instance automáticamente
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Suavizado")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Límites del nivel (opcional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Vector3 velocity = Vector3.zero;
    private bool isFollowing = true;

    private void Start()
    {
        if (target == null && Player.Instance != null)
        {
            target = Player.Instance.transform;
        }
    }

    private void LateUpdate()
    {
        if (!isFollowing || target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// asigna manualmente el objetivo a seguir (por si no es el Player, o cambia en tiempo de ejecución)
    /// </summary>
    public void SetTarget(Transform newTarget) => target = newTarget;

    /// <summary>
    /// deja de seguir al objetivo (usado durante teletransportes, intros de jefe, etc.)
    /// </summary>
    public void StopFollow() => isFollowing = false;

    /// <summary>
    /// reanuda el seguimiento
    /// </summary>
    public void StartFollow() => isFollowing = true;

    /// <summary>
    /// centra la cámara al instante en el objetivo, sin el suavizado (útil tras un teletransporte)
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        Vector3 pos = target.position + offset;
        if (useBounds)
        {
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        }
        transform.position = pos;
        velocity = Vector3.zero;
    }
}
