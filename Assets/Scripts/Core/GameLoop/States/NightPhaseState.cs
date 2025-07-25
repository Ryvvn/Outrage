using UnityEngine;

namespace Core.GameLoop.States
{
    /// <summary>
    /// Night phase state - Preparation for incoming wave.
    /// Players make final preparations before combat begins.
    /// Creates tension through visible countdown to wave start.
    /// </summary>
    public class NightPhaseState : GameLoopState
    {
        private bool _warningTriggered = false;
        
        /// <summary>
        /// Gets the phase this state represents.
        /// </summary>
        public override GamePhase Phase => GamePhase.Night;
        
        /// <summary>
        /// Called when entering the night phase.
        /// Sets up the environment for final preparations before combat.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Reset warning flag
            _warningTriggered = false;
            
            // Enable night-specific systems
            EnableFinalPreparations();
            SetNightLighting();
            ShowWaveWarning();
            
            Debug.Log("Night phase started - Wave incoming, make final preparations!");
        }
        
        /// <summary>
        /// Called every frame during the night phase.
        /// Monitors preparation activities and builds tension for incoming wave.
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (!IsActive) return;
            
            // Update night lighting progression
            UpdateNightLighting();
            
            // Trigger warnings as wave approaches
            TriggerWaveWarnings();
            
            // Update tension effects
            UpdateTensionEffects();
        }
        
        /// <summary>
        /// Called when exiting the night phase.
        /// Cleans up preparation systems and prepares for wave combat.
        /// </summary>
        public override void Exit()
        {
            // Disable night-specific systems
            DisableFinalPreparations();
            HideWaveWarning();
            
            Debug.Log("Night phase ended - Wave begins now!");
            
            base.Exit();
        }
        
        /// <summary>
        /// Enables final preparation systems during the night phase.
        /// Limited actions compared to build phase.
        /// </summary>
        private void EnableFinalPreparations()
        {
            // TODO: Enable limited preparation actions (tower upgrades, ability selection, etc.)
            // This will be implemented when preparation systems are added
            Debug.Log("Final preparation systems enabled - Limited actions available");
        }
        
        /// <summary>
        /// Disables preparation systems when leaving night phase.
        /// </summary>
        private void DisableFinalPreparations()
        {
            // TODO: Disable preparation actions
            // This will be implemented when preparation systems are added
            Debug.Log("Final preparation systems disabled");
        }
        
        /// <summary>
        /// Sets the lighting to night configuration.
        /// </summary>
        private void SetNightLighting()
        {
            if (config != null)
            {
                RenderSettings.ambientLight = config.nightAmbientColor;
                Debug.Log("Night lighting applied");
            }
        }
        
        /// <summary>
        /// Updates the lighting progression during the night phase.
        /// Creates an ominous atmosphere as the wave approaches.
        /// </summary>
        private void UpdateNightLighting()
        {
            if (config == null) return;
            
            // Calculate lighting progression based on phase progress
            float lightProgress = Progress;
            
            // Apply curve for smooth transition
            if (config.lightTransitionCurve != null)
            {
                lightProgress = config.lightTransitionCurve.Evaluate(lightProgress);
            }
            
            // Make lighting slightly more ominous as wave approaches
            Color baseNightColor = config.nightAmbientColor;
            Color ominousColor = baseNightColor * 0.8f; // Darker and more threatening
            
            Color currentColor = Color.Lerp(baseNightColor, ominousColor, lightProgress);
            RenderSettings.ambientLight = currentColor;
        }
        
        /// <summary>
        /// Shows wave warning UI and effects.
        /// </summary>
        private void ShowWaveWarning()
        {
            // TODO: Show wave warning UI, enemy preview, etc.
            // This will be implemented when UI systems are added
            Debug.Log("Wave warning displayed - Enemies approaching!");
        }
        
        /// <summary>
        /// Hides wave warning UI when leaving night phase.
        /// </summary>
        private void HideWaveWarning()
        {
            // TODO: Hide wave warning UI
            // This will be implemented when UI systems are added
            Debug.Log("Wave warning hidden");
        }
        
        /// <summary>
        /// Triggers escalating warnings as the wave approaches.
        /// </summary>
        private void TriggerWaveWarnings()
        {
            float timeRemaining = TimeRemaining;
            
            // Trigger warning at 30 seconds remaining
            if (!_warningTriggered && timeRemaining <= 30f)
            {
                _warningTriggered = true;
                Debug.Log("WARNING: Wave starts in 30 seconds!");
                
                // TODO: Play warning sound, flash UI, etc.
                // This will be implemented when audio and UI systems are added
            }
            
            // Trigger final warning at 10 seconds
            if (timeRemaining <= 10f && timeRemaining > 9f)
            {
                Debug.Log("FINAL WARNING: Wave starts in 10 seconds!");
                
                // TODO: Play urgent warning sound, intense UI effects
                // This will be implemented when audio and UI systems are added
            }
        }
        
        /// <summary>
        /// Updates tension-building effects during the night phase.
        /// </summary>
        private void UpdateTensionEffects()
        {
            // TODO: Add tension-building effects like:
            // - Distant enemy sounds
            // - Screen edge darkening
            // - Subtle screen shake
            // - Heartbeat-like audio
            // This will be implemented when audio and visual effects systems are added
            
            // For now, just log tension level
            float tensionLevel = Progress;
            if (tensionLevel >= 0.8f)
            {
                // High tension - wave is very close
            }
            else if (tensionLevel >= 0.5f)
            {
                // Medium tension - wave is approaching
            }
        }
    }
}
