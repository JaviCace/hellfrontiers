using UnityEngine;

namespace Level
{

/// <summary>
/// Puerta simple que se activa/desactiva físicamente, usada para bloquear o
/// desbloquear caminos en el mapa.
/// </summary>
public class MapDoor : Door
{
    protected override void Awake()
    {
        base.Awake();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        gameObject.SetActive(true);
        abrir = true; // estado inicial: abierta (desactivada)
    }

    public override void ChangeOpen()
    {
        abrir = !abrir;

        if (abrir) OpenDoor();
        else CloseDoor();
    }

    /// <summary>
    /// abre la puerta: desactiva su collider y la oculta
    /// </summary>
    public override void OpenDoor()
    {
        if (doorCollider != null) doorCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }

    /// <summary>
    /// cierra la puerta: activa su collider y la muestra
    /// </summary>
    public override void CloseDoor()
    {
        if (doorCollider != null) doorCollider.enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }
}
}
