using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// Clase base para los orbes recogibles. No tenía tu BaseOrb.js original, así que esta
/// versión está inferida de cómo la usan DamageOrb/JumpOrb/MoveSpeedOrb/ShieldOrb y Player.cs:
/// se recoge al tocar al jugador, guarda nombre/descripción, y expone OnActivate/OnDesactivate
/// para que cada orbe aplique su efecto. Si tu BaseOrb.js real difiere, pásamelo y lo ajusto.
/// </summary>
public class BaseOrb : MonoBehaviour
{
    [Header("Info del orbe")]
    [SerializeField] protected string orbName = "Orb";
    [SerializeField] protected string orbDescription = "";
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [Tooltip("Id único del orbe para el guardado. Si se deja vacío se usa el nombre")]
    [SerializeField] private string orbId = "";
    [Tooltip("Imagen del orbe para el menú del checkpoint. Si se deja vacía se usa el sprite del orbe")]
    [SerializeField] private Sprite icon;

    public string OrbName => orbName;
    public string OrbDescription => orbDescription;
    public string OrbId => string.IsNullOrEmpty(orbId) ? orbName : orbId;
    public Sprite Icon => icon != null ? icon : (spriteRenderer != null ? spriteRenderer.sprite : null);
    public bool IsCollected => collected;

    // referencia al jugador que lo recogió/activó (igual que "this.player" en tus orbes originales)
    protected Player player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        player = Player.Instance;
        player.CollectOrb(this);

        RemoveFromScene();
    }

    private bool collected;

    /// <summary>
    /// marca el orbe como ya recogido sin pasar por el jugador (al cargar una partida)
    /// </summary>
    public void MarkCollected()
    {
        collected = true;
        RemoveFromScene();
    }

    /// <summary>
    /// el orbe desaparece del mundo al recogerlo. No se destruye el objeto: el Player guarda la
    /// referencia (orbs / equippedOrbs) y sigue llamando a OnActivate/OnDesactivate sobre él.
    /// </summary>
    private void RemoveFromScene()
    {
        foreach (var col in GetComponents<Collider2D>()) col.enabled = false;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;
    }

    /// <summary>
    /// aplica el efecto del orbe (cada orbe lo sobreescribe)
    /// </summary>
    public virtual void OnActivate(Player p)
    {
        player = p;
    }

    /// <summary>
    /// quita el efecto del orbe (cada orbe lo sobreescribe)
    /// </summary>
    public virtual void OnDesactivate(Player p)
    {
        player = p;
    }

    /// <summary>
    /// tiñe el propio sprite del orbe (equivalente a this.setTint(...) en Phaser)
    /// </summary>
    protected void Tint(Color color)
    {
        if (spriteRenderer != null) spriteRenderer.color = color;
    }
}
}
