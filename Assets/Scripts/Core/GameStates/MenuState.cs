using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Services;

namespace Core.GameStates
{
    /// <summary>
    /// Main menu state that handles menu interactions and navigation.
    /// Provides options to start gameplay, access settings, and quit the game.
    /// </summary>
    public class MenuState : GameState
    {
        private const string MAIN_MENU_SCENE = "MainMenu";
        private bool _sceneLoaded = false;
        
        /// <summary>
        /// Called when entering the menu state.
        /// Loads the main menu scene if not already loaded.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Entering Menu State");
            
            _sceneLoaded = false;
            
            // Load main menu scene if not already loaded
            LoadMenuScene();
        }
        
        /// <summary>
        /// Updates the menu state and handles input.
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (!_sceneLoaded)
                return;
            
            // Handle menu input
            HandleMenuInput();
        }
        
        /// <summary>
        /// Called when exiting the menu state.
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            Debug.Log("Exiting Menu State");
        }
        
        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        private void LoadMenuScene()
        {
            var currentScene = SceneManager.GetActiveScene();
            
            if (currentScene.name == MAIN_MENU_SCENE)
            {
                _sceneLoaded = true;
                Debug.Log("Main menu scene already loaded");
                return;
            }
            
            Debug.Log("Loading main menu scene...");
            
            // Load the main menu scene
            var asyncOperation = SceneManager.LoadSceneAsync(MAIN_MENU_SCENE);
            if (asyncOperation != null)
            {
                asyncOperation.completed += OnMenuSceneLoaded;
            }
            else
            {
                Debug.LogWarning($"Could not load scene '{MAIN_MENU_SCENE}'. Scene may not be in build settings.");
                _sceneLoaded = true; // Continue anyway
            }
        }
        
        /// <summary>
        /// Called when the main menu scene has finished loading.
        /// </summary>
        /// <param name="asyncOperation">The async operation that completed.</param>
        private void OnMenuSceneLoaded(AsyncOperation asyncOperation)
        {
            _sceneLoaded = true;
            Debug.Log("Main menu scene loaded");
        }
        
        /// <summary>
        /// Handles input while in the menu state.
        /// </summary>
        private void HandleMenuInput()
        {
            // Handle keyboard shortcuts for testing
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitGame();
            }
        }
        
        /// <summary>
        /// Starts the game by transitioning to gameplay state.
        /// This method can be called by UI buttons or input handlers.
        /// </summary>
        public void StartGame()
        {
            Debug.Log("Starting game...");
            gameManager.ChangeState<GameplayState>();
        }
        
        /// <summary>
        /// Quits the game application.
        /// This method can be called by UI buttons or input handlers.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        /// <summary>
        /// Opens the settings menu.
        /// This method can be called by UI buttons.
        /// </summary>
        public void OpenSettings()
        {
            Debug.Log("Opening settings...");
            // TODO: Implement settings functionality
            // This could transition to a SettingsState or open a settings panel
        }
    }
}
