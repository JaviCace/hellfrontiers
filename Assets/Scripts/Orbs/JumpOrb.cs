using UnityEngine;
using player;

namespace Orbs
{

/// <summary>
/// Orbe que aumenta la altura de salto.
/// </summary>
public class JumpOrb : BaseOrb
{
    public override void OnActivate(Player p)
    {
        base.OnActivate(p);
        player.jumpSpeedModifier = 1.2f;                       // salto más fuerte
        player.orbTint = new Color(0.47f, 0.86f, 0.47f);       // color verde
        Tint(player.orbTint);
    }

    public override void OnDesactivate(Player p)
    {
        base.OnDesactivate(p);
        player.jumpSpeedModifier = 1f;                         // salto normal
    }
}
}
