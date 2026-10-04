/// <summary>
/// Clase base para los estados de una máquina de estados.
/// Cada estado hereda de esta clase y redefine Enter, Execute y Exit.
/// </summary>
/// <typeparam name="T">Tipo del contexto que usa el estado (Player, Enemy, etc.)</typeparam>
/// 
namespace State
{
    public abstract class BaseState<T>
{
    // referencia a la máquina que usa este estado
    public StateMachine<T> stateMachine;

    /// <summary>
    /// Se ejecuta al entrar en el estado.
    /// </summary>
    /// <param name="context">objeto que usa el estado</param>
    /// <param name="data">datos opcionales de entrada (equivalente al "data" de setState en Phaser)</param>
    public virtual void Enter(T context, object data = null) { }

    /// <summary>
    /// Se ejecuta cada frame mientras este estado esté activo.
    /// </summary>
    /// <param name="context">objeto que usa el estado</param>
    /// <param name="time">tiempo actual</param>
    /// <param name="delta">tiempo entre frames</param>
    public virtual void Execute(T context, float time, float delta) { }

    /// <summary>
    /// Se ejecuta antes de salir del estado.
    /// </summary>
    /// <param name="context">objeto que usa el estado</param>
    public virtual void Exit(T context) { }
}
}