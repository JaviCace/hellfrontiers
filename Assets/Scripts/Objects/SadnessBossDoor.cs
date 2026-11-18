using UnityEngine;
using Managers;

namespace Level
{

/// <summary>
/// Puerta que lleva a la batalla contra el jefe Tristeza.
/// Solo se abre si los tres botones de colores están activos.
/// </summary>
public class SadnessBossDoor : DoorBoss
{
    /// <summary>
    /// intenta abrir la puerta: solo abre si todos los botones de colores están activos
    /// </summary>
    public override void OpenDoor()
    {
        var buttons = PlayerDataManager.data.buttonStatus;

        if (buttons.blue && buttons.green && buttons.red)
        {
            base.OpenDoor(); // si están activos, lógica de apertura de DoorBoss (teletransporte)
        }
        else
        {
            Debug.Log("No se puede abrir la puerta: faltan botones");
            ShowMessage("Activate all the buttons to pass");
        }
    }
}
}
