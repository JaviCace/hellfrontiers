using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Starters;
using Orbs;
using player;

namespace Managers
{

/// <summary>
/// Menú del checkpoint para elegir los 2 orbes equipados (con las imágenes de los orbes recogidos).
///
/// Montaje (Canvas):
///   - Pon este script en el Canvas (NO en el panel, para que Awake se ejecute aunque el panel esté oculto).
///   - panel:           el panel del menú (empieza oculto)
///   - listContainer:   objeto con un Grid/Horizontal Layout Group donde se crean los orbes
///   - orbButtonPrefab: prefab de un Button con una Image (el script le pone la imagen del orbe)
///   - slotButtons[2]:  los dos huecos de equipado (Button con Image). Al pulsar uno se elige en qué hueco se equipa
///   - slotIcons[2]:    la Image donde se enseña el orbe equipado en cada hueco
///   - closeButton:     Button "Cerrar" -> OnClick = OrbSelectMenu.Close
/// </summary>
public class OrbSelectMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform listContainer;
    [SerializeField] private Button orbButtonPrefab;

    [Header("Huecos equipados (2)")]
    [SerializeField] private Button[] slotButtons = new Button[2];
    [SerializeField] private Image[] slotIcons = new Image[2];

    [Header("Colores")]
    [SerializeField] private Color selectedSlotColor = new Color(1f, 0.9f, 0.4f, 1f);
    [SerializeField] private Color normalSlotColor = Color.white;
    [SerializeField] private Color equippedOrbColor = new Color(1f, 1f, 1f, 0.4f);   // orbes de la lista ya equipados

    private Player player;
    private int selectedSlot;
    private readonly List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i;
            if (slotButtons[i] != null) slotButtons[i].onClick.AddListener(() => SelectSlot(slot));
        }
        if (panel != null) panel.SetActive(false);
    }

    public bool IsOpen => panel != null && panel.activeSelf;

    /// <summary>
    /// abre el menú y pausa el juego
    /// </summary>
    public void Open(Player p)
    {
        if (p == null || panel == null) return;
        player = p;
        selectedSlot = 0;

        panel.SetActive(true);
        GameFlow.MenuOpen = true;
        Time.timeScale = 0f;

        Refresh();
    }

    /// <summary>
    /// cierra el menú, guarda los orbes elegidos y reanuda el juego
    /// </summary>
    public void Close()
    {
        if (!IsOpen) return;

        PlayerDataManager.SaveOrbs(player);

        panel.SetActive(false);
        GameFlow.MenuOpen = false;
        Time.timeScale = LifecycleGuard.IsPaused ? 0f : 1f;
    }

    private void SelectSlot(int slot)
    {
        selectedSlot = slot;
        Refresh();
    }

    private void Equip(BaseOrb orb)
    {
        BaseOrb[] eq = { player.equippedOrbs[0], player.equippedOrbs[1] };
        int other = 1 - selectedSlot;

        if (eq[selectedSlot] == orb)
        {
            eq[selectedSlot] = null;                        // pulsar el orbe ya puesto en este hueco lo quita
        }
        else
        {
            if (eq[other] == orb) eq[other] = eq[selectedSlot];   // estaba en el otro hueco: se intercambian
            eq[selectedSlot] = orb;
        }

        player.SetEquippedOrbs(eq[0], eq[1]);
        Refresh();
    }

    private void Refresh()
    {
        // huecos equipados
        for (int i = 0; i < 2; i++)
        {
            BaseOrb orb = player.equippedOrbs[i];
            if (i < slotIcons.Length && slotIcons[i] != null)
            {
                slotIcons[i].sprite = orb != null ? orb.Icon : null;
                slotIcons[i].enabled = orb != null && slotIcons[i].sprite != null;
                slotIcons[i].preserveAspect = true;
            }
            if (i < slotButtons.Length && slotButtons[i] != null && slotButtons[i].targetGraphic != null)
                slotButtons[i].targetGraphic.color = (i == selectedSlot) ? selectedSlotColor : normalSlotColor;
        }

        // lista de orbes recogidos
        foreach (var go in spawned) if (go != null) Destroy(go);
        spawned.Clear();

        if (listContainer == null || orbButtonPrefab == null) return;

        foreach (BaseOrb orb in player.orbs)
        {
            if (orb == null) continue;
            BaseOrb captured = orb;

            Button b = Instantiate(orbButtonPrefab, listContainer);
            spawned.Add(b.gameObject);

            Image img = b.targetGraphic as Image;
            if (img == null) img = b.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.sprite = orb.Icon;
                img.preserveAspect = true;
                bool equipped = orb == player.equippedOrbs[0] || orb == player.equippedOrbs[1];
                img.color = equipped ? equippedOrbColor : Color.white;
            }

            b.onClick.AddListener(() => Equip(captured));
        }
    }
}
}
