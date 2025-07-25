using System;
using UnityEngine;
using Core.Services;

namespace Core.Time
{
    /// <summary>
    /// Interface for managing day/night visual transitions and timing.
    /// Provides visual feedback for the game loop phases through lighting changes.
    /// </summary>
    public interface IDayNightCycle : IService
    {
        /// <summary>
        /// Gets the current day progress (0.0 = start of day, 1.0 = end of day).
        /// </summary>
        float DayProgress { get; }
        
        /// <summary>
        /// Gets whether it is currently day time.
        /// </summary>
        bool IsDay { get; }
        
        /// <summary>
        /// Gets the current ambient light color.
        /// </summary>
        Color CurrentAmbientColor { get; }
        
        /// <summary>
        /// Gets the current time of day as a normalized value (0.0 to 1.0).
        /// 0.0 = midnight, 0.25 = dawn, 0.5 = noon, 0.75 = dusk, 1.0 = midnight
        /// </summary>
        float TimeOfDay { get; }
        
        /// <summary>
        /// Sets the time of day to a specific normalized value.
        /// </summary>
        /// <param name="normalizedTime">Time of day (0.0 to 1.0)</param>
        void SetTimeOfDay(float normalizedTime);
        
        /// <summary>
        /// Smoothly transitions to a specific time of day over a duration.
        /// </summary>
        /// <param name="targetTime">Target time of day (0.0 to 1.0)</param>
        /// <param name="duration">Duration of the transition in seconds</param>
        void TransitionToTimeOfDay(float targetTime, float duration);
        
        /// <summary>
        /// Updates the day/night cycle based on the current game phase.
        /// </summary>
        /// <param name="phase">Current game phase</param>
        /// <param name="phaseProgress">Progress within the current phase (0.0 to 1.0)</param>
        void UpdateForGamePhase(Core.GameLoop.GamePhase phase, float phaseProgress);
        
        /// <summary>
        /// Pauses the day/night cycle progression.
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resumes the day/night cycle progression.
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Event fired when the time of day changes.
        /// Parameter: normalizedTime (0.0 to 1.0)
        /// </summary>
        event Action<float> OnTimeOfDayChanged;
        
        /// <summary>
        /// Event fired when transitioning between day and night.
        /// Parameter: isDay (true for day, false for night)
        /// </summary>
        event Action<bool> OnDayNightTransition;
    }
}
