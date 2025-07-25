using UnityEngine;
using Core.GameLoop;
using Core.Time;
using Core.Services;
using Data;

namespace Core.GameLoop
{
    /// <summary>
    /// Example script demonstrating how to set up and use the game loop system.
    /// This can be attached to a GameObject in a scene to test the game loop functionality.
    /// </summary>
    public class GameLoopExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameLoopConfig gameLoopConfig;
        
        [Header("Components")]
        [SerializeField] private Light sunLight;
        
        [Header("Debug")]
        [SerializeField] private bool autoStartLoop = true;
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private bool logPhaseChanges = true;
        
        private GameLoopManager _gameLoopManager;
        private IDayNightCycle _dayNightManager;
        private bool _isSetup = false;
        
        private void Start()
        {
            SetupGameLoop();
            
            if (autoStartLoop)
            {
                StartGameLoop();
            }
        }
        
        /// <summary>
        /// Sets up the game loop system components.
        /// </summary>
        private void SetupGameLoop()
        {
            if (_isSetup) return;
            
            // Create GameLoopManager if it doesn't exist
            _gameLoopManager = FindObjectOfType<GameLoopManager>();
            if (_gameLoopManager == null)
            {
                var gameLoopObject = new GameObject("GameLoopManager");
                _gameLoopManager = gameLoopObject.AddComponent<GameLoopManager>();
                
                // Set the config if provided
                if (gameLoopConfig != null)
                {
                    var configField = typeof(GameLoopManager).GetField("config", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    configField?.SetValue(_gameLoopManager, gameLoopConfig);
                }
            }
            
            // Create DayNightCycleManager if it doesn't exist
            var dayNightManagerComponent = FindObjectOfType<DayNightCycleManager>();
            if (dayNightManagerComponent == null)
            {
                var dayNightObject = new GameObject("DayNightCycleManager");
                dayNightManagerComponent = dayNightObject.AddComponent<DayNightCycleManager>();
                
                // Set the config and sun light if provided
                if (gameLoopConfig != null)
                {
                    var configField = typeof(DayNightCycleManager).GetField("config", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    configField?.SetValue(dayNightManagerComponent, gameLoopConfig);
                }
                
                if (sunLight != null)
                {
                    var sunLightField = typeof(DayNightCycleManager).GetField("sunLight", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    sunLightField?.SetValue(dayNightManagerComponent, sunLight);
                }
            }
            _dayNightManager = (IDayNightCycle)dayNightManagerComponent;
            
            // Initialize the managers
            _gameLoopManager.Initialize();
            _dayNightManager.Initialize();
            
            // Subscribe to events for debugging
            if (logPhaseChanges)
            {
                _gameLoopManager.OnPhaseChanged += OnPhaseChanged;
                _gameLoopManager.OnPhaseTimeUpdated += OnPhaseTimeUpdated;
                _dayNightManager.OnDayNightTransition += OnDayNightTransition;
            }
            
            _isSetup = true;
            Debug.Log("Game loop system setup completed.");
        }
        
        /// <summary>
        /// Starts the game loop.
        /// </summary>
        public void StartGameLoop()
        {
            if (!_isSetup)
            {
                SetupGameLoop();
            }
            
            if (_gameLoopManager != null && !_gameLoopManager.IsInitialized)
            {
                _gameLoopManager.StartGameLoop();
                Debug.Log("Game loop started.");
            }
        }
        
        /// <summary>
        /// Stops the game loop.
        /// </summary>
        public void StopGameLoop()
        {
            if (_gameLoopManager != null && _gameLoopManager.IsInitialized)
            {
                _gameLoopManager.StopGameLoop();
                Debug.Log("Game loop stopped.");
            }
        }
        
        /// <summary>
        /// Pauses or resumes the game loop.
        /// </summary>
        public void TogglePause()
        {
            if (_gameLoopManager == null) return;
            
            if (_gameLoopManager.IsPaused)
            {
                _gameLoopManager.ResumeGameLoop();
                Debug.Log("Game loop resumed.");
            }
            else
            {
                _gameLoopManager.PauseGameLoop();
                Debug.Log("Game loop paused.");
            }
        }
        
        /// <summary>
        /// Forces a transition to the next phase.
        /// </summary>
        public void ForceNextPhase()
        {
            if (_gameLoopManager == null) return;
            
            var currentPhase = _gameLoopManager.CurrentPhase;
            var nextPhase = GetNextPhase(currentPhase);
            
            _gameLoopManager.ForcePhaseTransition(nextPhase);
            Debug.Log($"Forced transition from {currentPhase} to {nextPhase}.");
        }
        
        /// <summary>
        /// Gets the next phase in the sequence.
        /// </summary>
        /// <param name="currentPhase">Current phase</param>
        /// <returns>Next phase</returns>
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
        
        /// <summary>
        /// Event handler for phase changes.
        /// </summary>
        /// <param name="fromPhase">Previous phase</param>
        /// <param name="toPhase">New phase</param>
        private void OnPhaseChanged(GamePhase fromPhase, GamePhase toPhase)
        {
            Debug.Log($"Phase changed: {fromPhase} -> {toPhase}");
        }
        
        /// <summary>
        /// Event handler for time updates.
        /// </summary>
        /// <param name="timeRemaining">Time remaining in current phase</param>
        private void OnPhaseTimeUpdated(float timeRemaining)
        {
            // Only log occasionally to avoid spam
            if (UnityEngine.Time.frameCount % 60 == 0) // Every 60 frames
            {
                Debug.Log($"Phase time remaining: {timeRemaining:F1}s");
            }
        }
        
        /// <summary>
        /// Event handler for day/night transitions.
        /// </summary>
        /// <param name="isDay">Whether it's now day time</param>
        private void OnDayNightTransition(bool isDay)
        {
            Debug.Log($"Day/Night transition: {(isDay ? "Day" : "Night")}");
        }
        
        /// <summary>
        /// Debug GUI for testing.
        /// </summary>
        private void OnGUI()
        {
            if (!showDebugUI || !_isSetup) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Game Loop Debug", GUI.skin.box);
            
            if (_gameLoopManager != null)
            {
                GUILayout.Label($"Phase: {_gameLoopManager.CurrentPhase}");
                GUILayout.Label($"Time Remaining: {_gameLoopManager.PhaseTimeRemaining:F1}s");
                GUILayout.Label($"Progress: {_gameLoopManager.PhaseProgress:F2}");
                GUILayout.Label($"Initialized: {_gameLoopManager.IsInitialized}");
                GUILayout.Label($"Paused: {_gameLoopManager.IsPaused}");
                
                GUILayout.Space(10);
                
                if (GUILayout.Button(_gameLoopManager.IsInitialized ? "Stop Loop" : "Start Loop"))
                {
                    if (_gameLoopManager.IsInitialized)
                        StopGameLoop();
                    else
                        StartGameLoop();
                }
                
                if (GUILayout.Button(_gameLoopManager.IsPaused ? "Resume" : "Pause"))
                {
                    TogglePause();
                }
                
                if (GUILayout.Button("Next Phase"))
                {
                    ForceNextPhase();
                }
            }
            
            GUILayout.Space(10);
            
            if (_dayNightManager != null)
            {
                GUILayout.Label($"Time of Day: {_dayNightManager.TimeOfDay:F2}");
                GUILayout.Label($"Day Progress: {_dayNightManager.DayProgress:F2}");
                GUILayout.Label($"Is Day: {_dayNightManager.IsDay}");
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Set Dawn"))
                {
                    _dayNightManager.SetTimeOfDay(0.25f);
                }
                
                if (GUILayout.Button("Set Noon"))
                {
                    _dayNightManager.SetTimeOfDay(0.5f);
                }
                
                if (GUILayout.Button("Set Dusk"))
                {
                    _dayNightManager.SetTimeOfDay(0.75f);
                }
                
                if (GUILayout.Button("Set Midnight"))
                {
                    _dayNightManager.SetTimeOfDay(0.0f);
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Cleanup when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (_gameLoopManager != null)
            {
                _gameLoopManager.OnPhaseChanged -= OnPhaseChanged;
                _gameLoopManager.OnPhaseTimeUpdated -= OnPhaseTimeUpdated;
            }
            
            if (_dayNightManager != null)
            {
                _dayNightManager.OnDayNightTransition -= OnDayNightTransition;
            }
        }
    }
}
