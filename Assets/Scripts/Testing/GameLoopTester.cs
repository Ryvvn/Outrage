using UnityEngine;
using Core.GameLoop;
using Core.Services;
using Core.Time;

namespace Testing
{
    /// <summary>
    /// Simple testing script for the game loop system.
    /// Provides keyboard shortcuts to test different aspects of the game loop.
    /// </summary>
    public class GameLoopTester : MonoBehaviour
    {
        [Header("Testing Controls")]
        [SerializeField] private bool enableKeyboardControls = true;
        [SerializeField] private KeyCode startLoopKey = KeyCode.Space;
        [SerializeField] private KeyCode pauseResumeKey = KeyCode.P;
        [SerializeField] private KeyCode nextPhaseKey = KeyCode.N;
        [SerializeField] private KeyCode resetKey = KeyCode.R;
        
        [Header("Debug Info")]
        [SerializeField] private bool showDebugInfo = true;
        
        private IGameLoopManager _gameLoopManager;
        private IDayNightCycle _dayNightCycle;
        private bool _isInitialized = false;
        private GameObject _createdGameLoopManagerObj; // Track created GameObject for cleanup
        
        private void Start()
        {
            // Wait a frame to allow other components to initialize first
            StartCoroutine(InitializeWithDelay());
        }
        
        private System.Collections.IEnumerator InitializeWithDelay()
        {
            // Wait a frame to allow GameLoopManager in scene to initialize
            yield return null;
            
            // Try multiple times with small delays to find existing service
            int attempts = 0;
            const int maxAttempts = 10;
            
            while (attempts < maxAttempts)
            {
                _gameLoopManager = ServiceLocator.GetService<IGameLoopManager>();
                if (_gameLoopManager != null)
                {
                    break;
                }
                
                attempts++;
                yield return new WaitForSeconds(0.1f); // Wait 100ms between attempts
            }
            
            try
            {
                // If still no service found after waiting, only create in editor/development
                if (_gameLoopManager == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning("GameLoopManager service not found after waiting. Creating one for testing purposes.");
                    _createdGameLoopManagerObj = new GameObject("GameLoopManager_Tester");
                    var gameLoopManager = _createdGameLoopManagerObj.AddComponent<GameLoopManager>();
                    
                    // Don't destroy this object when loading new scenes (for testing)
                    DontDestroyOnLoad(_createdGameLoopManagerObj);
                    
                    gameLoopManager.Initialize();

                    // Try getting the service again after registration
                    _gameLoopManager = ServiceLocator.GetService<IGameLoopManager>();
#else
                    Debug.LogError("GameLoopManager service not found. GameLoopTester requires proper service initialization.");
                    yield break;
#endif
                }

                // Similarly, ensure the DayNightCycle service is available
                _dayNightCycle = ServiceLocator.GetService<IDayNightCycle>();
                if (_dayNightCycle == null)
                {
                    Debug.LogWarning("DayNightCycle service not found. Some functionality will be limited.");
                }

                if (_gameLoopManager != null)
                {
                    _isInitialized = true;
                    Debug.Log("GameLoopTester initialized successfully");

                    if (showDebugInfo)
                    {
                        _gameLoopManager.OnPhaseChanged += OnPhaseChanged;
                    }
                }
                else
                {
                    Debug.LogError("Failed to initialize GameLoopManager even after creation attempt. Tester will not function.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize GameLoopTester: {ex.Message}");
            }
        }


        private void Update()
        {
            if (!_isInitialized || !enableKeyboardControls)
                return;
                
            HandleKeyboardInput();
        }
        
        private void HandleKeyboardInput()
        {
            // Start/restart game loop
            if (Input.GetKeyDown(startLoopKey))
            {
                StartGameLoop();
            }
            
            // Pause/resume game loop
            if (Input.GetKeyDown(pauseResumeKey))
            {
                TogglePause();
            }
            
            // Force next phase
            if (Input.GetKeyDown(nextPhaseKey))
            {
                ForceNextPhase();
            }
            
            // Reset to day phase
            if (Input.GetKeyDown(resetKey))
            {
                ResetToDay();
            }
        }
        
        [ContextMenu("Start Game Loop")]
        public void StartGameLoop()
        {
            if (_gameLoopManager != null)
            {
                _gameLoopManager.StartGameLoop();
                Debug.Log("Game loop started via tester");
            }
        }
        
        [ContextMenu("Toggle Pause")]
        public void TogglePause()
        {
            if (_gameLoopManager != null)
            {
                if (_gameLoopManager.IsPaused)
                {
                    _gameLoopManager.ResumeGameLoop();
                    Debug.Log("Game loop resumed");
                }
                else
                {
                    _gameLoopManager.PauseGameLoop();
                    Debug.Log("Game loop paused");
                }
            }
        }
        
        [ContextMenu("Force Next Phase")]
        public void ForceNextPhase()
        {
            if (_gameLoopManager != null)
            {
                GamePhase nextPhase = GetNextPhase(_gameLoopManager.CurrentPhase);
                _gameLoopManager.ForcePhaseTransition(nextPhase);
                Debug.Log($"Forced transition to {nextPhase}");
            }
        }
        
        [ContextMenu("Reset to Day")]
        public void ResetToDay()
        {
            if (_gameLoopManager != null)
            {
                _gameLoopManager.ForcePhaseTransition(GamePhase.Day);
                Debug.Log("Reset to Day phase");
            }
        }
        
        private GamePhase GetNextPhase(GamePhase currentPhase)
        {
            return currentPhase switch
            {
                GamePhase.Day => GamePhase.Build,
                GamePhase.Build => GamePhase.Night,
                GamePhase.Night => GamePhase.WaveInProgress,
                GamePhase.WaveInProgress => GamePhase.EndWave,
                GamePhase.EndWave => GamePhase.Day,
                _ => GamePhase.Day
            };
        }
        
        private void OnPhaseChanged(GamePhase previousPhase, GamePhase newPhase)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[GameLoopTester] Phase changed: {previousPhase} → {newPhase}");
                
                if (_dayNightCycle != null)
                {
                    Debug.Log($"[GameLoopTester] Day progress: {_dayNightCycle.DayProgress:P1}, Is Day: {_dayNightCycle.IsDay}");
                }
            }
        }
        
        private void OnGUI()
        {
            if (!_isInitialized || !showDebugInfo)
                return;
                
            // Display testing instructions
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Game Loop Tester", GUI.skin.box);
            GUILayout.Label($"Space: Start Loop");
            GUILayout.Label($"P: Pause/Resume");
            GUILayout.Label($"N: Next Phase");
            GUILayout.Label($"R: Reset to Day");
            
            if (_gameLoopManager != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Current Phase: {_gameLoopManager.CurrentPhase}");
                GUILayout.Label($"Time Remaining: {_gameLoopManager.PhaseTimeRemaining:F1}s");
                GUILayout.Label($"Progress: {_gameLoopManager.PhaseProgress:P1}");
                GUILayout.Label($"Paused: {_gameLoopManager.IsPaused}");
            }
            
            GUILayout.EndArea();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            try
            {
                if (_gameLoopManager != null && showDebugInfo)
                {
                    _gameLoopManager.OnPhaseChanged -= OnPhaseChanged;
                }
            }
            catch (System.Exception ex)
            {
                // Ignore errors during shutdown - services might already be destroyed
                Debug.LogWarning($"GameLoopTester cleanup warning: {ex.Message}");
            }
            
            // Clean up created GameObject if we created one
            if (_createdGameLoopManagerObj != null)
            {
                try
                {
                    if (Application.isPlaying)
                    {
                        Destroy(_createdGameLoopManagerObj);
                    }
                    else
                    {
                        DestroyImmediate(_createdGameLoopManagerObj);
                    }
                }
                catch (System.Exception ex)
                {
                    // Ignore errors during shutdown
                    Debug.LogWarning($"GameLoopTester GameObject cleanup warning: {ex.Message}");
                }
                finally
                {
                    _createdGameLoopManagerObj = null;
                }
            }
        }
    }
}