using State;

namespace player
{

/// <summary>
/// Estado idle del jugador.
/// </summary>
public class PlayerIdleState : BaseState<Player>
{
    /// <summary>
    /// se ejecuta al entrar en idle
    /// </summary>
    public override void Enter(Player player, object data = null)
    {
        player.PlayAnim("Player_idle", true);      // animación idle
        player.SetVelocityX(0);                    // detener movimiento horizontal
    }

    /// <summary>
    /// lógica idle por frame
    /// </summary>
    public override void Execute(Player player, float time, float delta)
    {
        // si pulsa movimiento -> ir a move
        if (player.moveLeftHeld || player.moveRightHeld)
        {
            player.stateMachine.SetState("move");
            return;
        }

        // si puede saltar -> pasar a jump
        if (player.jumpBufferTimer > 0f && player.IsGrounded())
        {
            player.stateMachine.SetState("jump");
            return;
        }
    }
}
}
