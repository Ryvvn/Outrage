using UnityEngine;
using Core.Services;

namespace Core.GameStates
{
    /// <summary>
    /// Initial game setup state that handles service initialization and loading.
    /// Automatically transitions to MenuState when initialization is complete.
    /// </summary>
    public class InitializationState : GameState
    {
        private bool _initializationComplete = false;
        private float _initializationTimer = 0f;
        private const float MIN_INITIALIZATION_TIME = 0.1f; // Minimum time to show initialization
        
        /// <summary>
        /// Called when entering the initialization state.
        /// Starts the game initialization process.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Entering Initialization State");
            
            _initializationComplete = false;
            _initializationTimer = 0f;
            
            // Start initialization process
            StartInitialization();
        }
        
        /// <summary>
        /// Updates the initialization state and checks for completion.
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            _initializationTimer += UnityEngine.Time.deltaTime;
            
            // Check if initialization is complete and minimum time has passed
            if (_initializationComplete && _initializationTimer >= MIN_INITIALIZATION_TIME)
            {
                // Transition to menu state
                gameManager.ChangeState<MenuState>();
            }
        }
        
        /// <summary>
        /// Called when exiting the initialization state.
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            Debug.Log("Exiting Initialization State");
        }
        
        /// <summary>
        /// Starts the game initialization process.
        /// This includes setting up core services and loading essential data.
        /// </summary>
        private void StartInitialization()
        {
            Debug.Log("Starting game initialization...");
            
            try
            {
                // Initialize core services here
                // For now, we'll just mark initialization as complete
                // Future services like AudioManager, InputManager, SaveManager would be initialized here
                
                Debug.Log("Core services initialized");
                
                // Mark initialization as complete
                _initializationComplete = true;
                Debug.Log("Game initialization complete");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during initialization: {ex.Message}");
                // In a real implementation, you might want to show an error screen
                // For now, we'll still transition to menu
                _initializationComplete = true;
            }
        }
    }
}
