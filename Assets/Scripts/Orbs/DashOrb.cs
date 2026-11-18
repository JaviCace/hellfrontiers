using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// orbe que permite hacer dash
/// </summary>
public class DashOrb : BaseOrb
{
    // valores por defecto al añadir el componente (luego se pueden cambiar en el Inspector)
    private void Reset()
    {
        orbName = "Orb Dash";
        orbDescription = "Te permite hacer dash";
    }

    public override void OnActivate(Player p)
    {
        base.OnActivate(p);

        p.canDash = true;                  // habilitar dash
        p.SetTint(new Color32(160, 32, 240, 255));
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);

        p.canDash = false;                 // desactivar dash
    }
}
}
