using UnityEngine;

namespace Core.GameLoop.States
{
    /// <summary>
    /// Day phase state - Resource collection and preparation activities.
    /// Players can gather resources and prepare for the upcoming build phase.
    /// </summary>
    public class DayPhaseState : GameLoopState
    {
        /// <summary>
        /// Gets the phase this state represents.
        /// </summary>
        public override GamePhase Phase => GamePhase.Day;
        
        /// <summary>
        /// Called when entering the day phase.
        /// Sets up the environment for resource collection activities.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Enable day-specific systems
            EnableResourceCollection();
            SetDayLighting();
            
            Debug.Log("Day phase started - Resource collection enabled");
        }
        
        /// <summary>
        /// Called every frame during the day phase.
        /// Monitors resource collection and day progression.
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (!IsActive) return;
            
            // Update day lighting progression
            UpdateDayLighting();
            
            // Monitor resource collection
            MonitorResourceCollection();
        }
        
        /// <summary>
        /// Called when exiting the day phase.
        /// Cleans up day-specific systems and prepares for build phase.
        /// </summary>
        public override void Exit()
        {
            // Disable day-specific systems
            DisableResourceCollection();
            
            Debug.Log("Day phase ended - Transitioning to build phase");
            
            base.Exit();
        }
        
        /// <summary>
        /// Enables resource collection systems during the day phase.
        /// </summary>
        private void EnableResourceCollection()
        {
            // TODO: Enable resource spawning and collection mechanics
            // This will be implemented when resource systems are added
            Debug.Log("Resource collection systems enabled");
        }
        
        /// <summary>
        /// Disables resource collection systems when leaving day phase.
        /// </summary>
        private void DisableResourceCollection()
        {
            // TODO: Disable resource spawning and collection mechanics
            // This will be implemented when resource systems are added
            Debug.Log("Resource collection systems disabled");
        }
        
        /// <summary>
        /// Sets the lighting to day configuration.
        /// </summary>
        private void SetDayLighting()
        {
            if (config != null)
            {
                RenderSettings.ambientLight = config.dayAmbientColor;
                Debug.Log("Day lighting applied");
            }
        }
        
        /// <summary>
        /// Updates the lighting progression during the day phase.
        /// Creates a smooth transition as the day progresses.
        /// </summary>
        private void UpdateDayLighting()
        {
            if (config == null) return;
            
            // Calculate lighting progression based on phase progress
            float lightProgress = Progress;
            
            // Apply curve for smooth transition
            if (config.lightTransitionCurve != null)
            {
                lightProgress = config.lightTransitionCurve.Evaluate(lightProgress);
            }
            
            // Interpolate between day and slightly dimmer day lighting
            Color currentColor = Color.Lerp(config.dayAmbientColor, 
                                          config.dayAmbientColor * 0.9f, 
                                          lightProgress);
            
            RenderSettings.ambientLight = currentColor;
        }
        
        /// <summary>
        /// Monitors resource collection activities during the day phase.
        /// </summary>
        private void MonitorResourceCollection()
        {
            // TODO: Monitor resource collection progress, spawning, etc.
            // This will be implemented when resource systems are added
            
            // For now, just log progress occasionally
            if (UnityEngine.Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                Debug.Log($"Day phase progress: {Progress:F2} - Resources available for collection");
            }
        }
    }
}
