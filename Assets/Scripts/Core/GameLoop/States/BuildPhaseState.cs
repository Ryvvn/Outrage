using UnityEngine;

namespace Core.GameLoop.States
{
    /// <summary>
    /// Build phase state - Tower placement and strategic planning.
    /// Players can place towers and plan their defense strategy without time pressure.
    /// </summary>
    public class BuildPhaseState : GameLoopState
    {
        /// <summary>
        /// Gets the phase this state represents.
        /// </summary>
        public override GamePhase Phase => GamePhase.Build;
        
        /// <summary>
        /// Called when entering the build phase.
        /// Sets up the environment for tower placement and strategic planning.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Enable build-specific systems
            EnableTowerPlacement();
            EnableStrategicPlanning();
            SetBuildLighting();
            
            Debug.Log("Build phase started - Tower placement enabled");
        }
        
        /// <summary>
        /// Called every frame during the build phase.
        /// Monitors building activities and phase progression.
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (!IsActive) return;
            
            // Update build phase lighting
            UpdateBuildLighting();
            
            // Monitor building activities
            MonitorBuildingActivities();
        }
        
        /// <summary>
        /// Called when exiting the build phase.
        /// Cleans up build-specific systems and prepares for night phase.
        /// </summary>
        public override void Exit()
        {
            // Disable build-specific systems
            DisableTowerPlacement();
            DisableStrategicPlanning();
            
            Debug.Log("Build phase ended - Transitioning to night phase");
            
            base.Exit();
        }
        
        /// <summary>
        /// Enables tower placement systems during the build phase.
        /// </summary>
        private void EnableTowerPlacement()
        {
            // TODO: Enable tower placement UI and mechanics
            // This will be implemented when tower systems are added
            Debug.Log("Tower placement systems enabled");
        }
        
        /// <summary>
        /// Disables tower placement systems when leaving build phase.
        /// </summary>
        private void DisableTowerPlacement()
        {
            // TODO: Disable tower placement UI and mechanics
            // This will be implemented when tower systems are added
            Debug.Log("Tower placement systems disabled");
        }
        
        /// <summary>
        /// Enables strategic planning tools and UI during the build phase.
        /// </summary>
        private void EnableStrategicPlanning()
        {
            // TODO: Enable strategic planning UI (tower range indicators, path preview, etc.)
            // This will be implemented when strategic planning systems are added
            Debug.Log("Strategic planning tools enabled");
        }
        
        /// <summary>
        /// Disables strategic planning tools when leaving build phase.
        /// </summary>
        private void DisableStrategicPlanning()
        {
            // TODO: Disable strategic planning UI
            // This will be implemented when strategic planning systems are added
            Debug.Log("Strategic planning tools disabled");
        }
        
        /// <summary>
        /// Sets the lighting to build phase configuration.
        /// Slightly dimmer than day to indicate progression toward night.
        /// </summary>
        private void SetBuildLighting()
        {
            if (config != null)
            {
                // Build phase uses a blend between day and night lighting
                Color buildColor = Color.Lerp(config.dayAmbientColor, config.nightAmbientColor, 0.3f);
                RenderSettings.ambientLight = buildColor;
                Debug.Log("Build phase lighting applied");
            }
        }
        
        /// <summary>
        /// Updates the lighting progression during the build phase.
        /// Creates a smooth transition from day toward night.
        /// </summary>
        private void UpdateBuildLighting()
        {
            if (config == null) return;
            
            // Calculate lighting progression based on phase progress
            float lightProgress = Progress;
            
            // Apply curve for smooth transition
            if (config.lightTransitionCurve != null)
            {
                lightProgress = config.lightTransitionCurve.Evaluate(lightProgress);
            }
            
            // Interpolate from day-like to evening-like lighting
            Color startColor = Color.Lerp(config.dayAmbientColor, config.nightAmbientColor, 0.2f);
            Color endColor = Color.Lerp(config.dayAmbientColor, config.nightAmbientColor, 0.5f);
            
            Color currentColor = Color.Lerp(startColor, endColor, lightProgress);
            RenderSettings.ambientLight = currentColor;
        }
        
        /// <summary>
        /// Monitors building activities during the build phase.
        /// </summary>
        private void MonitorBuildingActivities()
        {
            // TODO: Track tower placement, resource usage, and strategic decisions
            // This will be implemented when tower and resource systems are added
            
            // For now, just log progress occasionally
            if (UnityEngine.Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                Debug.Log($"Build phase progress: {Progress:F2} - Tower placement available");
            }
            
            // Provide milestone feedback
            if (Progress >= 0.5f && Progress < 0.51f)
            {
                Debug.Log("Build phase halfway complete - Consider finalizing your strategy");
            }
            else if (Progress >= 0.8f && Progress < 0.81f)
            {
                Debug.Log("Build phase nearly complete - Prepare for night phase");
            }
        }
        
        /// <summary>
        /// Monitors building progress and provides feedback to the player.
        /// </summary>
        private void MonitorBuildingProgress()
        {
            // Legacy method - kept for compatibility
            MonitorBuildingActivities();
        }
    }
}
