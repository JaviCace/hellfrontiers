using UnityEngine;
using Managers;

namespace Level
{

/// <summary>
/// Indicador visual asociado a un botón (la clase "show" del original).
/// Implementa esto en tu script de indicador real, o dime su nombre y lo ajusto.
/// </summary>
public interface IButtonIndicator
{
    bool IsOn { get; set; }
    void ChangeTexture();
}

/// <summary>
/// Botón interactivo que activa puertas o mecanismos.
/// </summary>
public class Button : MonoBehaviour
{
    public enum ButtonColor { Rojo, Azul, Verde }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite pressedSprite;         // textura al presionar
    [SerializeField] private ButtonColor color;

    [SerializeField] private Door door;                     // puerta asociada (referencia informativa, igual que en el original)
    [SerializeField] private MonoBehaviour showIndicatorBehaviour; // debe implementar IButtonIndicator

    private bool pressed;

    private IButtonIndicator show => showIndicatorBehaviour as IButtonIndicator;

    private void Start()
    {
        // al cargar una partida, los botones ya activados aparecen pulsados
        var status = PlayerDataManager.data.buttonStatus;
        bool on = (color == ButtonColor.Rojo && status.red)
               || (color == ButtonColor.Azul && status.blue)
               || (color == ButtonColor.Verde && status.green);
        if (!on) return;

        pressed = true;
        ChangeTexture();
        if (show != null) { show.IsOn = true; show.ChangeTexture(); }
    }

    public void SetDoor(Door d) => door = d;
    public void SetShow(MonoBehaviour indicator) => showIndicatorBehaviour = indicator;

    /// <summary>
    /// cambia la textura del botón
    /// </summary>
    public void ChangeTexture()
    {
        if (spriteRenderer != null && pressedSprite != null) spriteRenderer.sprite = pressedSprite;
    }

    /// <summary>
    /// acción al presionar el botón: actualiza estados globales y visuales
    /// </summary>
    public void Press()
    {
        if (pressed) return;   // ya activado
        pressed = true;

        if (show != null) show.IsOn = true;

        // la puerta es solo informativa: aunque no esté asignada, el botón se activa igualmente
        if (door == null) Debug.LogWarning($"Button '{name}': no tiene puerta asignada (se activa igualmente).");

        ChangeTexture();
        Debug.Log($"Button '{name}' activado: {color}");

        switch (color)
        {
            case ButtonColor.Rojo:
                PlayerDataManager.data.buttonStatus.red = true;
                break;
            case ButtonColor.Azul:
                PlayerDataManager.data.buttonStatus.blue = true;
                break;
            case ButtonColor.Verde:
                PlayerDataManager.data.buttonStatus.green = true;
                break;
        }

        show?.ChangeTexture();
    }
}
}
