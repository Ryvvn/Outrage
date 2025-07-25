using UnityEngine;
using Core.Services;
using Core.GameStates;

namespace Core.Bootstrap
{
    /// <summary>
    /// Game initialization and service setup component.
    /// This should be placed on a GameObject in the Bootstrap scene.
    /// Handles the initial setup of all core services and starts the game state machine.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Settings")]
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private float _initializationDelay = 0.1f;
        
        private bool _isInitialized = false;
        
        private void Start()
        {
            if (_autoStart)
            {
                // Add a small delay to ensure all Awake methods have been called
                Invoke(nameof(InitializeGame), _initializationDelay);
            }
        }
        
        /// <summary>
        /// Initializes the game by setting up core services and starting the state machine.
        /// Can be called manually if autoStart is disabled.
        /// </summary>
        public void InitializeGame()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("Game is already initialized");
                return;
            }
            
            Debug.Log("Starting game bootstrap...");
            
            try
            {
                // Initialize ServiceLocator (this creates the singleton instance)
                var serviceLocator = ServiceLocator.Instance;
                Debug.Log("ServiceLocator initialized");
                
                // Create and initialize GameManager
                var gameManagerGO = new GameObject("GameManager");
                var gameManager = gameManagerGO.AddComponent<GameManager>();
                
                // Make GameManager persistent across scenes
                DontDestroyOnLoad(gameManagerGO);
                
                gameManager.Initialize();
                
                Debug.Log("Core services initialized");
                
                // Start the game state machine with initialization state
                gameManager.ChangeState<InitializationState>();
                
                _isInitialized = true;
                Debug.Log("Game bootstrap completed successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during game bootstrap: {ex.Message}\n{ex.StackTrace}");
                
                // Show error and quit in builds, stop play mode in editor
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
        
        /// <summary>
        /// Manually shuts down the game and all services.
        /// Useful for testing or when implementing a restart function.
        /// </summary>
        public void ShutdownGame()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Game is not initialized");
                return;
            }
            
            Debug.Log("Shutting down game...");
            
            try
            {
                // Shutdown all services through ServiceLocator
                ServiceLocator.ShutdownAllServices();
                
                _isInitialized = false;
                Debug.Log("Game shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during game shutdown: {ex.Message}");
            }
        }
        
        private void OnApplicationQuit()
        {
            if (_isInitialized)
            {
                ShutdownGame();
            }
        }
        
        private void OnDestroy()
        {
            if (_isInitialized)
            {
                ShutdownGame();
            }
        }
        
        /// <summary>
        /// Gets whether the game has been initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
    }
}
