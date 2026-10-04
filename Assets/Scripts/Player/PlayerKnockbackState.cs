using State;

namespace player
{

/// <summary>
/// Estado knockback del jugador: cuando recibe daño y es empujado.
/// </summary>
public class PlayerKnockbackState : BaseState<Player>
{
    /// <summary>
    /// entrar al estado knockback
    /// </summary>
    /// <param name="data">dirección del knockback (int), pasada en stateMachine.SetState("knockback", direction)</param>
    public override void Enter(Player player, object data = null)
    {
        int direction = data is int d ? d : 1;

        player.SetVelocity(
            player.knockbackDistance * direction,   // empuje horizontal
            player.knockbackDistance                // empuje vertical (positivo = hacia arriba en Unity 2D)
        );

        // tras tiempo de knockback -> volver a idle
        player.SafeDelay(player.knockbackTime, () =>
        {
            player.stateMachine.SetState("idle");
        });
    }

    /// <summary>
    /// lógica por frame
    /// </summary>
    public override void Execute(Player player, float time, float delta) { }

    /// <summary>
    /// al salir del estado knockback
    /// </summary>
    public override void Exit(Player player) { }
}
}
