using State;

namespace player
{

/// <summary>
/// Estado dash del jugador.
/// Controla el movimiento rápido e invulnerable.
/// </summary>
public class PlayerDashState : BaseState<Player>
{
    private float dashSpawnShadowFrequency = 0.04f;   // frecuencia de sombras (segundos)

    private float previousGravityScale;               // gravedad previa
    private float previousVelX;                        // velocidad previa

    /// <summary>
    /// se ejecuta al entrar en el estado dash
    /// </summary>
    public override void Enter(Player player, object data = null)
    {
        player.isDashing = true;                        // activar estado dash
        player.invulnerable = true;                      // invulnerable mientras dash

        // guardar estado previo
        previousGravityScale = player.GravityScale;      // gravedad previa
        previousVelX = player.VelocityX;                 // velocidad previa

        // quitar gravedad temporalmente
        player.GravityScale = 0f;
        player.SetVelocityY(0);

        int direction = player.direction;                // dirección horizontal
        player.SetVelocityX(player.dashSpeed * direction); // velocidad final del dash

        // temporizador para terminar dash
        player.SetDashEndCoroutine(player.SafeDelay(player.dashDuration, () =>
        {
            player.isDashing = false;
        }));

        // TODO: semitransparente mientras dash (player.alpha = 0.5 en Phaser)
        // aplica esto sobre tu SpriteRenderer si quieres el mismo efecto visual

        // timer que genera sombras
        player.SetGhostTimerCoroutine(player.StartRepeatingTimer(dashSpawnShadowFrequency, () =>
        {
            if (player.isDashing) player.SpawnDashGhost();  // crear sombra si aún dasheando
        }));
    }

    /// <summary>
    /// se ejecuta cada frame mientras el jugador esté en dash
    /// </summary>
    public override void Execute(Player player, float time, float delta)
    {
        if (!player.isDashing)                            // si el dash termina
        {
            player.dashCooldownTimer = player.dashCooldown; // iniciar cooldown
            player.stateMachine.SetState("idle");           // volver a idle
        }
    }

    /// <summary>
    /// se ejecuta al salir del estado dash
    /// </summary>
    public override void Exit(Player player)
    {
        player.isDashing = false;                         // quitar estado dash
        player.GravityScale = previousGravityScale;        // restaurar gravedad
        player.SetVelocityY(0);                             // reset vertical
        player.SetVelocityX(previousVelX);                  // restaurar velocidad anterior

        player.invulnerable = false;                        // ya no invulnerable
        // TODO: recuperar opacidad (player.alpha = 1 en Phaser)

        player.StopRepeatingTimer(player.GhostTimerCoroutine); // cancelar sombras
    }
}
}
