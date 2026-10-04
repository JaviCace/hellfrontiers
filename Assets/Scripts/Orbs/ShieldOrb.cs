using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// Orbe que permite activar un escudo que bloquea el siguiente ataque.
/// </summary>
public class ShieldOrb : BaseOrb
{
    public override void OnActivate(Player p)
    {
        base.OnActivate(p);
        player.canShield = true;                               // habilitar escudo
        player.orbTint = new Color(0.88f, 1f, 1f);             // color azul muy claro
        Tint(player.orbTint);
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);
        player.canShield = false;                              // desactivar escudo
    }
}
}
