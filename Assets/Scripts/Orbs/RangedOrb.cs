using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// orbe que habilita el ataque a distancia del jugador
/// </summary>
public class RangedOrb : BaseOrb
{
    // valores por defecto al añadir el componente (luego se pueden cambiar en el Inspector)
    private void Reset()
    {
        orbName = "Orb Ranged";
        orbDescription = "Te permite hacer ataque a distancia";
    }

    public override void OnActivate(Player p)
    {
        base.OnActivate(p);

        p.canRangeAttack = true;           // habilitar ataque a distancia
        p.SetTint(new Color32(0, 143, 57, 255));
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);

        p.canRangeAttack = false;          // desactivar ataque a distancia
    }
}
}
