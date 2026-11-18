using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Starters;
using player;

namespace Managers
{

/// <summary>
/// Menú de trucos para pruebas: una lista de objetos de la escena y un botón por cada uno
/// que teletransporta al Player hasta él.
///
/// Montaje (Canvas):
///   - Pon este script en el Canvas (no en el panel).
///   - panel:           panel del menú (se oculta solo al empezar)
///   - listContainer:   objeto con un Vertical/Grid Layout Group (si hay muchos, ponlo dentro de un Scroll View -> Content)
///   - buttonPrefab:    prefab de un Button con un texto (TextMeshPro o Text) hijo
///   - Botón que abre/cierra el menú -> OnClick = CheatMenu.Toggle
///   - destinations:    la lista de objetos a los que se puede ir (nombre + objeto)
/// </summary>
public class CheatMenu : MonoBehaviour
{
    [Serializable]
    public class Destination
    {
        [Tooltip("Texto del botón. Si se deja vacío se usa el nombre del objeto")]
        public string label;
        public Transform target;
    }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform listContainer;
    [SerializeField] private Button buttonPrefab;
    [Tooltip("Botón que abre/cierra el menú. Se oculta en builds donde los trucos están desactivados")]
    [SerializeField] private GameObject toggleButton;

    [Header("Destinos")]
    [SerializeField] private List<Destination> destinations = new List<Destination>();
    [Tooltip("Desplazamiento respecto al objeto al aparecer (para no quedar dentro del suelo)")]
    [SerializeField] private Vector2 teleportOffset = new Vector2(0f, 1f);

    [Header("Opciones")]
    [SerializeField] private bool pauseWhileOpen = true;
    [SerializeField] private bool closeAfterTeleport = true;
    [Tooltip("Si se desmarca, el menú solo existe en el editor y en Development Builds")]
    [SerializeField] private bool allowInReleaseBuild = true;

    private bool Allowed => allowInReleaseBuild || Debug.isDebugBuild || Application.isEditor;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (toggleButton != null) toggleButton.SetActive(Allowed);
    }

    /// <summary>
    /// abre o cierra el menú (conéctalo al OnClick del botón de abrir)
    /// </summary>
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (!Allowed || panel == null || GameFlow.MenuOpen) return;

        BuildList();
        panel.SetActive(true);

        if (pauseWhileOpen)
        {
            GameFlow.MenuOpen = true;
            Time.timeScale = 0f;
        }
    }

    public void Close()
    {
        if (!IsOpen) return;

        panel.SetActive(false);

        if (pauseWhileOpen)
        {
            GameFlow.MenuOpen = false;
            Time.timeScale = LifecycleGuard.IsPaused ? 0f : 1f;
        }
    }

    /// <summary>
    /// teletransporta al jugador al destino
    /// </summary>
    public void TeleportTo(Transform target)
    {
        Player p = Player.Instance;
        if (p == null || target == null) return;

        Vector3 pos = target.position + (Vector3)teleportOffset;
        p.transform.position = new Vector3(pos.x, pos.y, p.transform.position.z);
        p.SetVelocity(0, 0);

        if (closeAfterTeleport) Close();
    }

    private void BuildList()
    {
        if (listContainer == null || buttonPrefab == null) return;

        // limpiar la lista anterior
        for (int i = listContainer.childCount - 1; i >= 0; i--)
            Destroy(listContainer.GetChild(i).gameObject);

        foreach (Destination d in destinations)
        {
            if (d == null || d.target == null) continue;
            Transform target = d.target;

            Button b = Instantiate(buttonPrefab, listContainer);
            SetLabel(b, string.IsNullOrEmpty(d.label) ? target.name : d.label);
            b.onClick.AddListener(() => TeleportTo(target));
        }
    }

    private static void SetLabel(Button b, string text)
    {
        var tmp = b.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null) { tmp.text = text; return; }

        var legacy = b.GetComponentInChildren<Text>(true);
        if (legacy != null) legacy.text = text;
    }
}
}
