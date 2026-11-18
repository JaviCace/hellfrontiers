using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{

/// <summary>
/// Flujo de partida: nueva partida, reiniciar y volver al menú principal.
/// Todo se hace recargando la escena actual (debe estar en Build Settings).
/// </summary>
public static class GameFlow
{
    /// <summary>
    /// si es true, al cargar la escena se muestra el menú principal (true al abrir el juego)
    /// </summary>
    public static bool ShowMainMenuOnLoad { get; private set; } = true;

    /// <summary>
    /// true mientras el menú principal o el de muerte están abiertos (el menú de pausa y el LifecycleGuard lo respetan)
    /// </summary>
    public static bool MenuOpen { get; set; }

    /// <summary>
    /// true cuando se ha pedido "Continuar": el Player aplica la partida guardada al arrancar la escena
    /// </summary>
    public static bool ContinueRequested { get; set; }

    /// <summary>
    /// se lanza al empezar una partida desde cero: engancha aquí lo que haya que borrar del progreso
    /// </summary>
    public static event System.Action OnResetProgress;

    // por si en el editor tienes desactivado el "Domain Reload" al entrar en Play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ShowMainMenuOnLoad = true;
        MenuOpen = false;
        ContinueRequested = false;
        OnResetProgress = null;
    }

    /// <summary>
    /// empieza el juego desde cero (botón "Nueva partida" y "Reiniciar")
    /// </summary>
    public static void NewGame()
    {
        ResetProgress();
        ShowMainMenuOnLoad = false;
        ReloadScene();
    }

    /// <summary>
    /// continúa desde el último checkpoint guardado (botón "Continuar")
    /// </summary>
    public static void Continue()
    {
        if (!PlayerDataManager.Load()) return;
        ContinueRequested = true;
        ShowMainMenuOnLoad = false;
        ReloadScene();
    }

    /// <summary>
    /// vuelve al menú principal (botón "Main menu" del menú de muerte)
    /// </summary>
    public static void GoToMainMenu()
    {
        ResetProgress(); // al volver al menú el progreso se pierde: así la escena carga limpia
        ShowMainMenuOnLoad = true;
        ReloadScene();
    }

    /// <summary>
    /// borra el progreso guardado (jefes derrotados, etc.)
    /// </summary>
    public static void ResetProgress()
    {
        PlayerDataManager.ResetData();
        OnResetProgress?.Invoke();
    }

    /// <summary>
    /// cierra la aplicación (en el editor para el modo Play)
    /// </summary>
    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void ReloadScene()
    {
        MenuOpen = false;
        Time.timeScale = 1f; // por si el juego estaba en pausa tras morir / con el menú abierto
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
}
