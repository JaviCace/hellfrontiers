using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// Orbe que aumenta la velocidad de movimiento.
/// </summary>
public class MoveSpeedOrb : BaseOrb
{
    public override void OnActivate(Player p)
    {
        base.OnActivate(p);
        player.speedMultiplier = 1.5f;                         // más velocidad
        player.orbTint = new Color(0.62f, 0.77f, 0.91f);       // color azul claro
        Tint(player.orbTint);
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);
        player.speedMultiplier = 1f;                           // velocidad normal
    }
}
}
