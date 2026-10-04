using Managers;

namespace Level
{

/// <summary>
/// Puerta que lleva a la batalla contra el jefe Tristeza.
/// Requiere que los tres botones de colores estén activos.
/// </summary>
public class SadnessBossDoor : DoorBoss
{
    /// <summary>
    /// intenta abrir la puerta; solo abre si todos los botones de colores están activos
    /// </summary>
    public override void OpenDoor()
    {
        if (PlayerDataManager.data.buttonStatus.blue
            && PlayerDataManager.data.buttonStatus.green
            && PlayerDataManager.data.buttonStatus.red)
        {
            base.OpenDoor();
        }
        else
        {
            ShowMessage("Activa todos los botones para poder pasar");
        }
    }
}
}
