using UnityEngine;

namespace Managers
{

/// <summary>
/// Menú de muerte: lo muestra el Player al morir.
/// Botones (conéctalos en el OnClick de cada Button):
///   Reiniciar -> DeathMenu.Restart()      empieza de cero
///   Main menu -> DeathMenu.GoToMainMenu() vuelve al menú principal
///
/// Ponlo en el Canvas (o en el propio panel) y asigna el panel de muerte en "Panel".
/// </summary>
public class DeathMenu : MonoBehaviour
{
    [Tooltip("Panel del menú de muerte. Si se deja vacío se usa este mismo objeto")]
    [SerializeField] private GameObject panel;

    [Tooltip("Botón Continuar: solo se muestra si hay una partida guardada")]
    [SerializeField] private GameObject continueButton;

    private static DeathMenu instance;

    /// <summary>
    /// el menú de muerte de la escena (lo busca, incluso si está desactivado)
    /// </summary>
    public static DeathMenu Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<DeathMenu>(FindObjectsInactive.Include);
            return instance;
        }
    }

    private GameObject Panel => panel != null ? panel : gameObject;

    private void Awake()
    {
        instance = this;
        Panel.SetActive(false); // empieza oculto
    }

    /// <summary>
    /// muestra el menú y pausa el juego
    /// </summary>
    public void Show()
    {
        if (continueButton != null) continueButton.SetActive(PlayerDataManager.HasSave);
        Panel.SetActive(true);
        GameFlow.MenuOpen = true;
        Time.timeScale = 0f;
    }

    public void Continue() => GameFlow.Continue();

    public void Restart() => GameFlow.NewGame();

    public void GoToMainMenu() => GameFlow.GoToMainMenu();

    /// <summary>
    /// botón "Salir"
    /// </summary>
    public void QuitGame() => GameFlow.Quit();
}
}
