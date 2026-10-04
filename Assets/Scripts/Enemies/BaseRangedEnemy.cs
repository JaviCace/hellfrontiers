namespace Enemies
{

/// <summary>
/// Enemigo terrestre a distancia simple.
/// </summary>
public class BaseRangedEnemy : BaseEnemy
{
    public float attackTime = 1f;                           // tiempo de carga del ataque (segundos)

    protected override void Awake()
    {
        base.Awake();                                       // inicializar BaseEnemy

        // ataque
        attackRange = 20f;                                  // rango ataque a distancia
        canChase = false;                                   // no se mueve, solo dispara

        // máquina de estados
        stateMachine
            .AddState("move", new GroundEnemyMoveState())    // estado mover
            .AddState("attack", new RangedEnemyAttackState()) // estado atacar
            .SetState("move");                                // estado inicial
    }
}
}
