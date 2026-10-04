using UnityEngine;

namespace Level
{

/// <summary>
/// Zona invisible para detectar eventos dentro de la sala del jefe.
/// Monta esto sobre un GameObject con un BoxCollider2D marcado como Is Trigger.
/// </summary>
public class BossRoom : MonoBehaviour
{
    [SerializeField] private BoxCollider2D zoneCollider;

    private void Awake()
    {
        if (zoneCollider != null) zoneCollider.isTrigger = true;
    }
}
}
