using UnityEngine;

namespace Core.GameLoop.States
{
    /// <summary>
    /// Wave in progress state - Combat and survival phase.
    /// Players defend against waves of enemies using their placed towers.
    /// This is the core combat phase of the game loop.
    /// </summary>
    public class WaveProgressState : GameLoopState
    {
        private int _enemiesRemaining;
        private bool _waveStarted = false;
        
        /// <summary>
        /// Gets the phase this state represents.
        /// </summary>
        public override GamePhase Phase => GamePhase.WaveInProgress;
        
        /// <summary>
        /// Gets the number of enemies remaining in the current wave.
        /// </summary>
        public int EnemiesRemaining => _enemiesRemaining;
        
        /// <summary>
        /// Gets whether the wave has been successfully completed.
        /// </summary>
        public bool IsWaveComplete => _enemiesRemaining <= 0 && _waveStarted;
        
        /// <summary>
        /// Called when entering the wave progress phase.
        /// Starts the combat wave and enables combat systems.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Initialize wave state
            _waveStarted = false;
            _enemiesRemaining = 0;
            
            // Enable combat systems
            EnableCombatSystems();
            StartWave();
            SetCombatLighting();
            
            Debug.Log("Wave progress phase started - Combat begins!");
        }
        
        /// <summary>
        /// Called every frame during the wave progress phase.
        /// Monitors combat progress and enemy status.
        /// </summary>
        public override void Update()
        {
            // Don't call base.Update() as we handle completion differently
            if (!IsActive || timer == null) return;
            
            // Update combat lighting
            UpdateCombatLighting();
            
            // Monitor wave progress
            MonitorWaveProgress();
            
            // Check for wave completion (either all enemies defeated or time expired)
            if (IsWaveComplete || timer.IsComplete)
            {
                OnPhaseComplete();
            }
        }
        
        /// <summary>
        /// Called when exiting the wave progress phase.
        /// Cleans up combat systems and prepares for end wave phase.
        /// </summary>
        public override void Exit()
        {
            // Disable combat systems
            DisableCombatSystems();
            EndWave();
            
            bool waveSuccessful = IsWaveComplete && !timer.IsComplete;
            Debug.Log($"Wave progress phase ended - Wave {(waveSuccessful ? "successful" : "failed")}");
            
            base.Exit();
        }
        
        /// <summary>
        /// Enables combat systems during the wave progress phase.
        /// </summary>
        private void EnableCombatSystems()
        {
            // TODO: Enable tower combat, enemy AI, projectile systems, etc.
            // This will be implemented when combat systems are added
            Debug.Log("Combat systems enabled");
        }
        
        /// <summary>
        /// Disables combat systems when leaving wave progress phase.
        /// </summary>
        private void DisableCombatSystems()
        {
            // TODO: Disable combat systems, clean up projectiles, etc.
            // This will be implemented when combat systems are added
            Debug.Log("Combat systems disabled");
        }
        
        /// <summary>
        /// Starts the enemy wave spawning.
        /// </summary>
        private void StartWave()
        {
            // TODO: Initialize wave spawning system
            // This will be implemented when enemy and wave systems are added
            
            // For now, simulate a wave with placeholder enemy count
            _enemiesRemaining = 20; // Placeholder value
            _waveStarted = true;
            
            Debug.Log($"Wave started with {_enemiesRemaining} enemies");
        }
        
        /// <summary>
        /// Ends the current wave and cleans up.
        /// </summary>
        private void EndWave()
        {
            // TODO: Clean up remaining enemies, projectiles, effects
            // This will be implemented when combat systems are added
            
            _waveStarted = false;
            _enemiesRemaining = 0;
            
            Debug.Log("Wave ended");
        }
        
        /// <summary>
        /// Sets the lighting to combat configuration.
        /// Intense lighting to emphasize the action.
        /// </summary>
        private void SetCombatLighting()
        {
            if (config != null)
            {
                // Combat uses slightly brighter night lighting for visibility
                Color combatColor = config.nightAmbientColor * 1.2f;
                RenderSettings.ambientLight = combatColor;
                Debug.Log("Combat lighting applied");
            }
        }
        
        /// <summary>
        /// Updates the lighting during combat to create dynamic atmosphere.
        /// </summary>
        private void UpdateCombatLighting()
        {
            if (config == null) return;
            
            // Create subtle pulsing effect during combat
            float pulseIntensity = Mathf.Sin(UnityEngine.Time.time * 2f) * 0.1f + 1f;
            Color baseColor = config.nightAmbientColor * 1.2f;
            Color currentColor = baseColor * pulseIntensity;
            
            RenderSettings.ambientLight = currentColor;
        }
        
        /// <summary>
        /// Monitors the progress of the current wave.
        /// </summary>
        private void MonitorWaveProgress()
        {
            // TODO: Track enemy spawning, deaths, player health, etc.
            // This will be implemented when combat systems are added
            
            // For now, simulate enemy deaths over time
            if (_waveStarted && _enemiesRemaining > 0)
            {
                // Simulate enemies being defeated over time (for testing)
                float waveProgress = Progress;
                int expectedEnemiesRemaining = Mathf.RoundToInt(20 * (1f - waveProgress));
                
                if (expectedEnemiesRemaining < _enemiesRemaining)
                {
                    _enemiesRemaining = expectedEnemiesRemaining;
                    Debug.Log($"Enemies remaining: {_enemiesRemaining}");
                }
            }
            
            // Log progress milestones
            if (Progress >= 0.25f && Progress < 0.26f)
            {
                Debug.Log("Wave 25% complete");
            }
            else if (Progress >= 0.5f && Progress < 0.51f)
            {
                Debug.Log("Wave 50% complete - Halfway there!");
            }
            else if (Progress >= 0.75f && Progress < 0.76f)
            {
                Debug.Log("Wave 75% complete - Almost finished!");
            }
        }
        
        /// <summary>
        /// Called when an enemy is defeated.
        /// This method will be called by the combat system.
        /// </summary>
        /// <param name="enemy">The defeated enemy (placeholder parameter)</param>
        public void OnEnemyDefeated(object enemy = null)
        {
            if (_enemiesRemaining > 0)
            {
                _enemiesRemaining--;
                Debug.Log($"Enemy defeated! Enemies remaining: {_enemiesRemaining}");
                
                // TODO: Award resources, update score, trigger effects
                // This will be implemented when reward systems are added
            }
        }
        
        /// <summary>
        /// Called when an enemy reaches the end of the path.
        /// This method will be called by the pathfinding system.
        /// </summary>
        /// <param name="enemy">The enemy that reached the end (placeholder parameter)</param>
        public void OnEnemyReachedEnd(object enemy = null)
        {
            if (_enemiesRemaining > 0)
            {
                _enemiesRemaining--;
                Debug.Log($"Enemy reached the end! Enemies remaining: {_enemiesRemaining}");
                
                // TODO: Reduce player health, trigger damage effects
                // This will be implemented when health and damage systems are added
            }
        }
    }
}
