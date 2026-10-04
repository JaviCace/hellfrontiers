using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Máquina de estados finita.
/// Controla qué estado está activo y ejecuta Enter/Exit/Execute.
/// </summary>
/// <typeparam name="T">Tipo del contexto que usa la máquina (Player, Enemy, etc.)</typeparam>
/// 
namespace State
{
    public class StateMachine<T>
{
    public string Id { get; private set; }                                    // identificador del sistema
    private readonly T context;                                               // objeto dueño de la máquina
    private readonly Dictionary<string, BaseState<T>> states = new Dictionary<string, BaseState<T>>(); // estados registrados

    public BaseState<T> CurrentState { get; private set; }                    // estado activo actual
    public BaseState<T> PreviousState { get; private set; }                   // estado previo

    /// <param name="context">objeto que usa la máquina (player, enemigo, etc.)</param>
    /// <param name="id">etiqueta de la máquina</param>
    public StateMachine(T context, string id)
    {
        this.context = context;
        Id = id;
    }

    /// <summary>
    /// Registra un nuevo estado en la máquina.
    /// </summary>
    public StateMachine<T> AddState(string name, BaseState<T> state)
    {
        states[name] = state;              // guardar en el diccionario
        state.stateMachine = this;         // asignar referencia inversa
        return this;                       // permite encadenar llamadas
    }

    /// <summary>
    /// Cambia el estado actual.
    /// </summary>
    /// <param name="name">nombre del nuevo estado</param>
    /// <param name="data">datos opcionales para Enter</param>
    public void SetState(string name, object data = null)
    {
        if (!states.TryGetValue(name, out BaseState<T> newState))
        {
            Debug.LogWarning("estado no encontrado: " + name);
            return;
        }

        // salir del estado anterior
        CurrentState?.Exit(context);

        PreviousState = CurrentState;      // guardar referencia
        CurrentState = newState;           // actualizar estado activo

        // entrar en el nuevo estado
        CurrentState.Enter(context, data);
    }

    /// <summary>
    /// Ejecuta el estado actual cada frame.
    /// </summary>
    public void Step(float time, float delta)
    {
        CurrentState?.Execute(context, time, delta);
    }

    /// <summary>
    /// Devuelve el nombre del estado activo actual.
    /// </summary>
    public string GetStateName()
    {
        foreach (var pair in states)
        {
            if (pair.Value == CurrentState) return pair.Key;
        }
        return null;
    }
}
}