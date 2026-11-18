using UnityEngine;
using UnityEngine.Events;
using Managers;
using player;

namespace Level
{

/// <summary>
/// Zona invisible que guarda la partida cuando el Player la toca.
/// Guarda igual que un checkpoint: posición de reaparición, jefes derrotados, botones y orbes.
/// Montaje: objeto con un Collider2D en modo Trigger que cubra la zona + este script.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SaveTrigger : MonoBehaviour
{
    [Tooltip("Dónde reaparecerá el jugador al continuar. Si se deja vacío se usa este objeto")]
    [SerializeField] private Transform respawnPoint;
    [Tooltip("Desplazamiento sobre el punto de reaparición (para no quedar dentro del suelo)")]
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.5f);

    [SerializeField] private bool healPlayer = false;
    [Tooltip("Solo guarda la primera vez que entra el jugador")]
    [SerializeField] private bool once = true;

    [Tooltip("Para enseñar un aviso 'Partida guardada' u otro feedback")]
    [SerializeField] private UnityEvent onSaved;

    private bool used;

    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && once) return;

        Player p = other.GetComponentInParent<Player>();
        if (p == null || p.dead) return;

        used = true;
        Save(p);
    }

    /// <summary>
    /// guarda la partida (también se puede llamar desde un evento)
    /// </summary>
    public void Save(Player p)
    {
        if (p == null) p = Player.Instance;
        if (p == null) return;

        Transform point = respawnPoint != null ? respawnPoint : transform;
        Vector2 respawn = (Vector2)point.position + respawnOffset;

        if (healPlayer) p.health = p.maxHealth;

        PlayerDataManager.SaveCheckpoint(p, respawn);
        Debug.Log("[SaveTrigger] Partida guardada en " + respawn);
        onSaved?.Invoke();
    }
}
}
