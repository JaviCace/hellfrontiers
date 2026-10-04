using UnityEngine;
using State;

namespace player
{

/// <summary>
/// Estado salto del jugador.
/// </summary>
public class PlayerJumpState : BaseState<Player>
{
    /// <summary>
    /// se ejecuta al iniciar el salto
    /// </summary>
    public override void Enter(Player player, object data = null)
    {
        player.PlayAnim("Player_jump", true);
        player.SetVelocityY(player.jumpSpeed * player.jumpSpeedModifier);          // aplicar salto
    }

    /// <summary>
    /// lógica del salto
    /// </summary>
    public override void Execute(Player player, float time, float delta)
    {
        // movimiento en el aire a la izquierda
        if (player.moveLeftHeld)
        {
            player.SetFlipX(true);
            player.direction = -1;
            player.SetVelocityX(player.direction * player.movementSpeed * player.speedMultiplier);
        }
        // movimiento en el aire a la derecha
        else if (player.moveRightHeld)
        {
            player.SetFlipX(false);
            player.direction = 1;
            player.SetVelocityX(player.direction * player.movementSpeed * player.speedMultiplier);
        }

        // si toca el suelo -> volver a idle o move
        if (player.IsGrounded())
        {
            if (Mathf.Abs(player.VelocityX) > 10f)
                player.stateMachine.SetState("move");
            else
                player.stateMachine.SetState("idle");
        }

        // pogo jump
        if (player.canPogoJump && player.jumpBufferTimer > 0f)
        {
            player.canPogoJump = false;
            player.SetVelocityY(player.pogoJumpSpeed * player.jumpSpeedModifier);
            player.PlayAnim("Player_jump", true);
        }
    }

    /// <summary>
    /// al salir del estado salto
    /// </summary>
    public override void Exit(Player player)
    {
        player.canPogoJump = false;       // quitar pogo jump
    }
}
}
