using UnityEngine;

namespace Core.GameLoop.States
{
    /// <summary>
    /// End wave state - Upgrade choices and progression.
    /// Players review wave results and make upgrade choices before the next cycle.
    /// </summary>
    public class EndWaveState : GameLoopState
    {
        private bool _upgradeChoiceMade = false;
        private bool _resultsShown = false;
        
        /// <summary>
        /// Gets the phase this state represents.
        /// </summary>
        public override GamePhase Phase => GamePhase.EndWave;
        
        /// <summary>
        /// Gets whether the player has made their upgrade choice.
        /// </summary>
        public bool UpgradeChoiceMade => _upgradeChoiceMade;
        
        /// <summary>
        /// Called when entering the end wave phase.
        /// Shows wave results and presents upgrade choices.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Initialize end wave state
            _upgradeChoiceMade = false;
            _resultsShown = false;
            
            // Enable end wave systems
            ShowWaveResults();
            EnableUpgradeSelection();
            SetPostCombatLighting();
            
            Debug.Log("End wave phase started - Review results and choose upgrades");
        }
        
        /// <summary>
        /// Called every frame during the end wave phase.
        /// Monitors upgrade selection and phase progression.
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (!IsActive) return;
            
            // Update post-combat lighting
            UpdatePostCombatLighting();
            
            // Monitor upgrade selection
            MonitorUpgradeSelection();
            
            // Show results after a brief delay
            if (!_resultsShown && Progress >= 0.1f)
            {
                DisplayDetailedResults();
                _resultsShown = true;
            }
        }
        
        /// <summary>
        /// Called when exiting the end wave phase.
        /// Applies selected upgrades and prepares for next cycle.
        /// </summary>
        public override void Exit()
        {
            // Apply upgrades and clean up
            ApplySelectedUpgrades();
            HideUpgradeSelection();
            HideWaveResults();
            
            Debug.Log("End wave phase ended - Starting new cycle");
            
            base.Exit();
        }
        
        /// <summary>
        /// Shows the wave results summary.
        /// </summary>
        private void ShowWaveResults()
        {
            // TODO: Display wave completion status, score, resources earned, etc.
            // This will be implemented when UI and scoring systems are added
            Debug.Log("Wave results displayed");
        }
        
        /// <summary>
        /// Displays detailed wave results after a brief delay.
        /// </summary>
        private void DisplayDetailedResults()
        {
            // TODO: Show detailed statistics, performance metrics, etc.
            // This will be implemented when statistics systems are added
            Debug.Log("Detailed wave results displayed");
        }
        
        /// <summary>
        /// Hides wave results when leaving end wave phase.
        /// </summary>
        private void HideWaveResults()
        {
            // TODO: Hide wave results UI
            // This will be implemented when UI systems are added
            Debug.Log("Wave results hidden");
        }
        
        /// <summary>
        /// Enables upgrade selection systems during the end wave phase.
        /// </summary>
        private void EnableUpgradeSelection()
        {
            // TODO: Show upgrade options, enable selection UI
            // This will be implemented when upgrade systems are added
            Debug.Log("Upgrade selection enabled - Choose your upgrades!");
        }
        
        /// <summary>
        /// Disables upgrade selection systems when leaving end wave phase.
        /// </summary>
        private void HideUpgradeSelection()
        {
            // TODO: Hide upgrade selection UI
            // This will be implemented when upgrade systems are added
            Debug.Log("Upgrade selection hidden");
        }
        
        /// <summary>
        /// Sets the lighting to post-combat configuration.
        /// Calmer lighting to allow for strategic thinking.
        /// </summary>
        private void SetPostCombatLighting()
        {
            if (config != null)
            {
                // Post-combat uses a blend between night and day lighting
                Color postCombatColor = Color.Lerp(config.nightAmbientColor, config.dayAmbientColor, 0.4f);
                RenderSettings.ambientLight = postCombatColor;
                Debug.Log("Post-combat lighting applied");
            }
        }
        
        /// <summary>
        /// Updates the lighting during the end wave phase.
        /// Gradually transitions toward day lighting for the next cycle.
        /// </summary>
        private void UpdatePostCombatLighting()
        {
            if (config == null) return;
            
            // Calculate lighting progression based on phase progress
            float lightProgress = Progress;
            
            // Apply curve for smooth transition
            if (config.lightTransitionCurve != null)
            {
                lightProgress = config.lightTransitionCurve.Evaluate(lightProgress);
            }
            
            // Gradually transition from post-combat to early day lighting
            Color startColor = Color.Lerp(config.nightAmbientColor, config.dayAmbientColor, 0.4f);
            Color endColor = Color.Lerp(config.nightAmbientColor, config.dayAmbientColor, 0.7f);
            
            Color currentColor = Color.Lerp(startColor, endColor, lightProgress);
            RenderSettings.ambientLight = currentColor;
        }
        
        /// <summary>
        /// Monitors upgrade selection progress.
        /// </summary>
        private void MonitorUpgradeSelection()
        {
            // TODO: Check if player has made upgrade choices
            // This will be implemented when upgrade systems are added
            
            // For now, simulate upgrade choice after some time
            if (!_upgradeChoiceMade && Progress >= 0.6f)
            {
                // Simulate upgrade choice for testing
                _upgradeChoiceMade = true;
                Debug.Log("Upgrade choice made (simulated)");
            }
            
            // Provide time warnings
            if (TimeRemaining <= 10f && TimeRemaining > 9f)
            {
                Debug.Log("10 seconds remaining to choose upgrades!");
            }
            else if (TimeRemaining <= 5f && TimeRemaining > 4f)
            {
                Debug.Log("5 seconds remaining - Choose quickly!");
            }
        }
        
        /// <summary>
        /// Applies the selected upgrades.
        /// </summary>
        private void ApplySelectedUpgrades()
        {
            // TODO: Apply player-selected upgrades to towers, abilities, etc.
            // This will be implemented when upgrade systems are added
            
            if (_upgradeChoiceMade)
            {
                Debug.Log("Selected upgrades applied");
            }
            else
            {
                Debug.Log("No upgrades selected - applying default progression");
            }
        }
        
        /// <summary>
        /// Called when the player selects an upgrade.
        /// This method will be called by the upgrade UI system.
        /// </summary>
        /// <param name="upgradeId">The ID of the selected upgrade</param>
        public void OnUpgradeSelected(string upgradeId)
        {
            // TODO: Process upgrade selection
            // This will be implemented when upgrade systems are added
            
            _upgradeChoiceMade = true;
            Debug.Log($"Upgrade selected: {upgradeId}");
        }
        
        /// <summary>
        /// Forces the phase to complete early if the player has made their choices.
        /// </summary>
        public void CompleteEarly()
        {
            if (_upgradeChoiceMade)
            {
                Debug.Log("Player completed upgrade selection early");
                OnPhaseComplete();
            }
        }
        
        /// <summary>
        /// Gets the wave completion statistics.
        /// This method will be used by the UI and statistics systems.
        /// </summary>
        /// <returns>Wave statistics (placeholder return type)</returns>
        public object GetWaveStatistics()
        {
            // TODO: Return actual wave statistics
            // This will be implemented when statistics systems are added
            
            return new
            {
                WaveNumber = 1, // Placeholder
                EnemiesDefeated = 20, // Placeholder
                ResourcesEarned = 100, // Placeholder
                TimeCompleted = timer?.ElapsedTime ?? 0f
            };
        }
    }
}
