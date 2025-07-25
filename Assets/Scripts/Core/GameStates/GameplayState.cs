using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Services;

namespace Core.GameStates
{
    /// <summary>
    /// Core gameplay state that handles the main tower defense game loop.
    /// Manages the Day → Build → Night → Survive → Upgrade cycle.
    /// </summary>
    public class GameplayState : GameState
    {
        private const string GAMEPLAY_SCENE = "Gameplay";
        private bool _sceneLoaded = false;
        private bool _gameplayInitialized = false;
        
        /// <summary>
        /// Called when entering the gameplay state.
        /// Loads the gameplay scene and initializes game systems.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Entering Gameplay State");
            
            _sceneLoaded = false;
            _gameplayInitialized = false;
            
            // Load gameplay scene
            LoadGameplayScene();
        }
        
        /// <summary>
        /// Updates the gameplay state and manages the game loop.
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (!_sceneLoaded || !_gameplayInitialized)
                return;
            
            // Handle gameplay input
            HandleGameplayInput();
            
            // Update game systems would go here
            // TODO: Implement tower defense game loop
        }
        
        /// <summary>
        /// Called when exiting the gameplay state.
        /// Cleans up gameplay resources and saves progress.
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            Debug.Log("Exiting Gameplay State");
            
            // Clean up gameplay resources
            CleanupGameplay();
        }
        
        /// <summary>
        /// Loads the gameplay scene.
        /// </summary>
        private void LoadGameplayScene()
        {
            var currentScene = SceneManager.GetActiveScene();
            
            if (currentScene.name == GAMEPLAY_SCENE)
            {
                _sceneLoaded = true;
                InitializeGameplay();
                Debug.Log("Gameplay scene already loaded");
                return;
            }
            
            Debug.Log("Loading gameplay scene...");
            
            // Load the gameplay scene
            var asyncOperation = SceneManager.LoadSceneAsync(GAMEPLAY_SCENE);
            if (asyncOperation != null)
            {
                asyncOperation.completed += OnGameplaySceneLoaded;
            }
            else
            {
                Debug.LogWarning($"Could not load scene '{GAMEPLAY_SCENE}'. Scene may not be in build settings.");
                _sceneLoaded = true;
                InitializeGameplay();
            }
        }
        
        /// <summary>
        /// Called when the gameplay scene has finished loading.
        /// </summary>
        /// <param name="asyncOperation">The async operation that completed.</param>
        private void OnGameplaySceneLoaded(AsyncOperation asyncOperation)
        {
            _sceneLoaded = true;
            Debug.Log("Gameplay scene loaded");
            
            // Initialize gameplay systems
            InitializeGameplay();
        }
        
        /// <summary>
        /// Initializes gameplay systems and prepares for the game loop.
        /// </summary>
        private void InitializeGameplay()
        {
            Debug.Log("Initializing gameplay systems...");
            
            try
            {
                // TODO: Initialize gameplay systems here:
                // - Level generation system
                // - Tower placement system
                // - Enemy spawning system
                // - Resource management system
                // - UI systems
                
                _gameplayInitialized = true;
                Debug.Log("Gameplay systems initialized");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error initializing gameplay: {ex.Message}");
                // Return to menu on initialization failure
                gameManager.ChangeState<MenuState>();
            }
        }
        
        /// <summary>
        /// Handles input during gameplay.
        /// </summary>
        private void HandleGameplayInput()
        {
            // Handle escape to pause/menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
            
            // Handle other gameplay input
            // TODO: Implement tower placement, camera controls, etc.
        }
        
        /// <summary>
        /// Pauses the game and shows pause menu.
        /// </summary>
        private void PauseGame()
        {
            Debug.Log("Game paused");
            
            // TODO: Implement pause functionality
            // This could:
            // - Set Time.timeScale = 0
            // - Show pause menu UI
            // - Allow returning to main menu
            
            // For now, return to menu
            ReturnToMenu();
        }
        
        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public void ReturnToMenu()
        {
            Debug.Log("Returning to main menu...");
            gameManager.ChangeState<MenuState>();
        }
        
        /// <summary>
        /// Cleans up gameplay resources when exiting the state.
        /// </summary>
        private void CleanupGameplay()
        {
            Debug.Log("Cleaning up gameplay resources...");
            
            // TODO: Cleanup gameplay systems:
            // - Save game progress
            // - Destroy temporary objects
            // - Reset time scale
            // - Clear event subscriptions
            
            UnityEngine.Time.timeScale = 1f; // Ensure time scale is reset
            
            _gameplayInitialized = false;
        }
    }
}
