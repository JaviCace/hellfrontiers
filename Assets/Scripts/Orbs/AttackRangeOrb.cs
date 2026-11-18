using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// orbe que aumenta el rango del ataque melee
/// </summary>
public class AttackRangeOrb : BaseOrb
{
    // valores por defecto al añadir el componente (luego se pueden cambiar en el Inspector)
    private void Reset()
    {
        orbName = "Orb AttackRange";
        orbDescription = "Realizas ataques melees mas anchos";
    }

    public override void OnActivate(Player p)
    {
        base.OnActivate(p);

        p.attackRangeMultiplier = 1.5f;   // aumentar alcance
        p.SetTint(new Color32(255, 222, 33, 255));
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);

        p.attackRangeMultiplier = 1f;     // volver a normal
    }
}
}
