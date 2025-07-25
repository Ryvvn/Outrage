using System;
using Core.Services;

namespace Core.GameLoop
{
    /// <summary>
    /// Interface for managing the core game loop state machine.
    /// Handles phase transitions, timing, and coordination with other systems.
    /// </summary>
    public interface IGameLoopManager : IService
    {
        /// <summary>
        /// Gets the current active game phase.
        /// </summary>
        GamePhase CurrentPhase { get; }
        
        /// <summary>
        /// Gets the time remaining in the current phase (in seconds).
        /// </summary>
        float PhaseTimeRemaining { get; }
        
        /// <summary>
        /// Gets the progress of the current phase (0.0 to 1.0).
        /// </summary>
        float PhaseProgress { get; }
        
        /// <summary>
        /// Gets whether the game loop is currently paused.
        /// </summary>
        bool IsPaused { get; }
        
        /// <summary>
        /// Starts the game loop from the Day phase.
        /// </summary>
        void StartGameLoop();
        
        /// <summary>
        /// Pauses the game loop timer.
        /// </summary>
        void PauseGameLoop();
        
        /// <summary>
        /// Resumes the game loop timer.
        /// </summary>
        void ResumeGameLoop();
        
        /// <summary>
        /// Forces an immediate transition to the specified phase.
        /// Use with caution - primarily for testing or special game events.
        /// </summary>
        /// <param name="targetPhase">The phase to transition to</param>
        void ForcePhaseTransition(GamePhase targetPhase);
        
        /// <summary>
        /// Event fired when the game phase changes.
        /// Parameters: (previousPhase, newPhase)
        /// </summary>
        event Action<GamePhase, GamePhase> OnPhaseChanged;
        
        /// <summary>
        /// Event fired when the phase time is updated.
        /// Parameter: timeRemaining (in seconds)
        /// </summary>
        event Action<float> OnPhaseTimeUpdated;
    }
}
