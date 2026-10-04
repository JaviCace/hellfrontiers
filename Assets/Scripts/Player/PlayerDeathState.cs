using UnityEngine;
using State;

namespace player
{

/// <summary>
/// Estado muerte del jugador.
/// </summary>
public class PlayerDeathState : BaseState<Player>
{
    /// <summary>
    /// se ejecuta al entrar en el estado muerte
    /// </summary>
    public override void Enter(Player player, object data = null)
    {
        player.SetVelocityX(0);                 // parar movimiento
        Object.Destroy(player.gameObject);       // destruir el GameObject del jugador
    }

    /// <summary>
    /// estado muerte no necesita lógica de execute
    /// </summary>
    public override void Execute(Player player, float time, float delta) { }
}
}
