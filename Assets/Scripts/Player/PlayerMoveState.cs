using State;

namespace player
{

/// <summary>
/// Estado movimiento del jugador.
/// </summary>
public class PlayerMoveState : BaseState<Player>
{
    /// <summary>
    /// entrar en movimiento
    /// </summary>
    public override void Enter(Player player, object data = null)
    {
        player.PlayAnim("Player_walk", true);      // animación caminar
    }

    /// <summary>
    /// actualizar movimiento
    /// </summary>
    public override void Execute(Player player, float time, float delta)
    {
        // si deja de moverse -> idle
        if (!player.moveLeftHeld && !player.moveRightHeld)
        {
            player.stateMachine.SetState("idle");
            return;
        }

        // movimiento a la izquierda
        if (player.moveLeftHeld)
        {
            player.direction = -1;
            player.SetFlipX(true);
        }
        // movimiento a la derecha
        else if (player.moveRightHeld)
        {
            player.direction = 1;
            player.SetFlipX(false);
        }

        // salto desde movimiento
        if (player.jumpBufferTimer > 0f && player.IsGrounded())
        {
            player.stateMachine.SetState("jump");
            return;
        }

        // aplicar velocidad horizontal
        player.SetVelocityX(player.direction * player.movementSpeed * player.speedMultiplier);
    }

    /// <summary>
    /// salir del estado movimiento
    /// </summary>
    public override void Exit(Player player)
    {
        player.SetVelocityX(0);            // parar personaje
    }
}
}
