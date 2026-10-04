using System.Collections.Generic;

namespace Managers
{

/// <summary>
/// Versión mínima inferida de tu PlayerDataManager real (no estaba entre los archivos
/// compartidos). Cubre exactamente lo que usan Button, Trigger y SadnessBossDoor:
/// estado de los 3 botones de color y qué jefes han sido derrotados.
/// Si tu manager real guarda más cosas (progreso, posición de respawn, etc.),
/// pásamelo y fusiono esto con tu versión completa.
/// </summary>
public static class PlayerDataManager
{
    public class ButtonStatusData
    {
        public bool red;
        public bool blue;
        public bool green;
    }

    public class GameData
    {
        public ButtonStatusData buttonStatus = new ButtonStatusData();
        public Dictionary<string, bool> bossStatus = new Dictionary<string, bool>();
    }

    public static GameData data = new GameData();

    /// <summary>
    /// marca un jefe como derrotado
    /// </summary>
    public static void KillBoss(string bossName)
    {
        if (string.IsNullOrEmpty(bossName)) return;
        data.bossStatus[bossName] = true;
    }

    /// <summary>
    /// comprueba si un jefe ya fue derrotado
    /// </summary>
    public static bool IsBossDefeated(string bossName)
    {
        return !string.IsNullOrEmpty(bossName) && data.bossStatus.TryGetValue(bossName, out bool dead) && dead;
    }
}
}
