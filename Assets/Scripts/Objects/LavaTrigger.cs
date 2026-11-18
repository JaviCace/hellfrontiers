using UnityEngine;
using player;

namespace Level
{

/// <summary>
/// Zona (BoxCollider2D en modo Trigger) que arranca o para la lava cuando entra el jugador.
/// La lava no hace nada hasta que un trigger la activa.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LavaTrigger : MonoBehaviour
{
    public enum LavaAction { Start, Stop }

    [SerializeField] private FloorIsLava lava;
    [SerializeField] private LavaAction action = LavaAction.Start;
    [Tooltip("Solo se activa la primera vez que entra el jugador")]
    [SerializeField] private bool once = true;

    private bool used;

    private void Reset()
    {
        // al añadir el script, deja el collider como trigger
        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && once) return;
        if (lava == null) return;
        if (other.GetComponentInParent<Player>() == null) return;

        used = true;
        if (action == LavaAction.Start) lava.StartLava();
        else lava.StopLava();
    }
}
}
