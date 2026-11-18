using UnityEngine;
using Starters;

namespace Managers
{

/// <summary>
/// Menú principal. Se muestra al abrir el juego (y al volver desde el menú de muerte), con el juego en pausa detrás.
/// Botón (conéctalo en el OnClick):
///   Nueva partida -> MainMenu.NewGame()   empieza de cero
///
/// Ponlo en el Canvas (o en el propio panel) y asigna el panel del menú en "Panel".
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Tooltip("Panel del menú principal. Si se deja vacío se usa este mismo objeto")]
    [SerializeField] private GameObject panel;

    [Tooltip("Botón Continuar: solo se muestra si hay una partida guardada")]
    [SerializeField] private GameObject continueButton;

    private GameObject Panel => panel != null ? panel : gameObject;

    private void Awake()
    {
        bool show = GameFlow.ShowMainMenuOnLoad;
        Panel.SetActive(show);
        GameFlow.MenuOpen = show;
        if (continueButton != null) continueButton.SetActive(PlayerDataManager.HasSave);
        Time.timeScale = show ? 0f : 1f; // pausado mientras el menú está abierto
    }

    /// <summary>
    /// cierra el menú y empieza: la escena ya está recién cargada, así que no hace falta recargarla
    /// </summary>
    public void Continue() => GameFlow.Continue();

    public void NewGame()
    {
        GameFlow.ResetProgress();
        Panel.SetActive(false);
        GameFlow.MenuOpen = false;
        LifecycleGuard.ClearPause(); // por si el guard se había quedado en pausa antes de empezar
        Time.timeScale = 1f;
    }

    /// <summary>
    /// botón "Salir"
    /// </summary>
    public void QuitGame() => GameFlow.Quit();
}
}
