using UnityEngine;
using Managers;

namespace Level
{

/// <summary>
/// Puerta que lleva a la batalla final.
/// Requiere haber derrotado a los jefes Ira y Tristeza.
/// </summary>
public class FinalBossDoor : DoorBoss
{
    [Header("Jefes requeridos (deben coincidir con bossName de cada jefe)")]
    [SerializeField] private string angerBossName = "anger";
    [SerializeField] private string sadnessBossName = "sadness";

    /// <summary>
    /// intenta abrir la puerta: solo abre si los jefes requeridos han sido derrotados
    /// </summary>
    public override void OpenDoor()
    {
        if (!PlayerDataManager.IsBossDefeated(sadnessBossName) || !PlayerDataManager.IsBossDefeated(angerBossName))
        {
            Debug.Log("No se puede abrir la puerta: te falta matar dos bosses");
            ShowMessage("You must defeat the bosses ANGER and SADNESS");
            return;
        }

        base.OpenDoor(); // si se cumplen los requisitos, lógica de apertura de DoorBoss
    }
}
}
