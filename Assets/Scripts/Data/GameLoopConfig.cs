using UnityEngine;

namespace Data
{
    /// <summary>
    /// Configuration for game loop timing and visual settings.
    /// ScriptableObject that allows easy balancing without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "GameLoopConfig", menuName = "Game/Game Loop Config")]
    public class GameLoopConfig : ScriptableObject
    {
        [Header("Phase Durations (seconds)")]
        [Tooltip("Duration of the day phase - resource gathering time")]
        public float dayPhaseDuration = 180f;     // 3 minutes
        
        [Tooltip("Duration of the build phase - strategic planning time")]
        public float buildPhaseDuration = 120f;   // 2 minutes
        
        [Tooltip("Duration of the night phase - preparation for combat")]
        public float nightPhaseDuration = 60f;    // 1 minute
        
        [Tooltip("Duration of the wave progress phase - combat duration")]
        public float waveProgressDuration = 300f; // 5 minutes
        
        [Tooltip("Duration of the end wave phase - upgrade selection time")]
        public float endWaveDuration = 30f;       // 30 seconds
        
        [Header("Day/Night Visual Settings")]
        [Tooltip("Ambient light color during day phases")]
        public Color dayAmbientColor = Color.white;
        
        [Tooltip("Ambient light color during night phases")]
        public Color nightAmbientColor = new Color(0.3f, 0.3f, 0.5f);
        
        [Tooltip("Curve controlling the transition between day and night lighting")]
        public AnimationCurve lightTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Advanced Settings")]
        [Tooltip("Enable smooth transitions between phases")]
        public bool enableSmoothTransitions = true;
        
        [Tooltip("Duration of transition effects between phases")]
        [Range(0.1f, 5f)]
        public float transitionDuration = 1f;
        
        /// <summary>
        /// Gets the total duration of one complete game loop cycle.
        /// </summary>
        public float TotalCycleDuration => 
            dayPhaseDuration + buildPhaseDuration + nightPhaseDuration + 
            waveProgressDuration + endWaveDuration;
        
        /// <summary>
        /// Gets the duration for a specific game phase.
        /// </summary>
        /// <param name="phase">The phase to get duration for</param>
        /// <returns>Duration in seconds</returns>
        public float GetPhaseDuration(Core.GameLoop.GamePhase phase)
        {
            return phase switch
            {
                Core.GameLoop.GamePhase.Day => dayPhaseDuration,
                Core.GameLoop.GamePhase.Build => buildPhaseDuration,
                Core.GameLoop.GamePhase.Night => nightPhaseDuration,
                Core.GameLoop.GamePhase.WaveInProgress => waveProgressDuration,
                Core.GameLoop.GamePhase.EndWave => endWaveDuration,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Validates the configuration values.
        /// </summary>
        private void OnValidate()
        {
            // Ensure all durations are positive
            dayPhaseDuration = Mathf.Max(1f, dayPhaseDuration);
            buildPhaseDuration = Mathf.Max(1f, buildPhaseDuration);
            nightPhaseDuration = Mathf.Max(1f, nightPhaseDuration);
            waveProgressDuration = Mathf.Max(1f, waveProgressDuration);
            endWaveDuration = Mathf.Max(1f, endWaveDuration);
            
            // Ensure transition duration is reasonable
            transitionDuration = Mathf.Clamp(transitionDuration, 0.1f, 5f);
            
            // Ensure light transition curve exists
            if (lightTransitionCurve == null || lightTransitionCurve.length == 0)
            {
                lightTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
    }
}