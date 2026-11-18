using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// orbe que cura al matar enemigos
/// </summary>
public class BloodStealOrb : BaseOrb
{
    // valores por defecto al añadir el componente (luego se pueden cambiar en el Inspector)
    private void Reset()
    {
        orbName = "Orb BloodSteal";
        orbDescription = "Cuando matas a un enemigo, curas 1 de vida";
    }

    public override void OnActivate(Player p)
    {
        base.OnActivate(p);

        p.bloodStealAmount = 1f;           // curar al matar
        p.SetTint(new Color32(139, 0, 0, 255));
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);

        p.bloodStealAmount = 0f;           // sin curación
    }
}
}
