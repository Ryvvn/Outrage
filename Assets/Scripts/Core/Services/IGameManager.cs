using System;
using Core.GameStates;

namespace Core.Services
{
    /// <summary>
    /// Interface for the game manager service that handles game state transitions.
    /// </summary>
    public interface IGameManager : IService
    {
        /// <summary>
        /// Gets the currently active game state.
        /// </summary>
        GameState CurrentState { get; }
        
        /// <summary>
        /// Changes to a new game state of the specified type.
        /// Creates a new instance if one doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of state to transition to.</typeparam>
        void ChangeState<T>() where T : GameState, new();
        
        /// <summary>
        /// Changes to a specific game state instance.
        /// </summary>
        /// <param name="newState">The state instance to transition to.</param>
        void ChangeState(GameState newState);
        
        /// <summary>
        /// Event fired when the game state changes.
        /// Parameters: (previousState, newState)
        /// </summary>
        event Action<GameState, GameState> OnStateChanged;
        
        /// <summary>
        /// Gets whether the game manager is currently transitioning between states.
        /// </summary>
        bool IsTransitioning { get; }
    }
}
