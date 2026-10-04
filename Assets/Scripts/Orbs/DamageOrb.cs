using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// Orbe que aumenta el daño del jugador.
/// </summary>
public class DamageOrb : BaseOrb
{
    public override void OnActivate(Player p)
    {
        base.OnActivate(p);
        player.damageMultiplier = 1.5f;                       // más daño
        player.orbTint = new Color(1f, 0.6f, 0f);              // color naranja
        Tint(player.orbTint);
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);
        player.damageMultiplier = 1f;                          // volver al normal
        player.canDash = false;                                // seguridad extra (no necesario, igual que en el original)
    }
}
}
