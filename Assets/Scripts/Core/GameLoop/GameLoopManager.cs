using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Services;
using Core.GameLoop.States;
using Core.Time;
using Data;

namespace Core.GameLoop
{
    /// <summary>
    /// Core game loop manager that handles phase transitions and timing.
    /// Implements the "Day → Build → Night → WaveInProgress → EndWave" gameplay cycle.
    /// </summary>
    public class GameLoopManager : MonoBehaviour, IGameLoopManager
    {
        [Header("Configuration")]
        [SerializeField] private GameLoopConfig config;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private GameLoopState _currentState;
        private Dictionary<GamePhase, GameLoopState> _states;
        private bool _isTransitioning;
        private bool _isPaused;
        private bool _isGameLoopActive;
        
        /// <summary>
        /// Gets the current active game phase.
        /// </summary>
        public GamePhase CurrentPhase => _currentState?.Phase ?? GamePhase.Day;
        
        /// <summary>
        /// Gets the time remaining in the current phase (in seconds).
        /// </summary>
        public float PhaseTimeRemaining => _currentState?.TimeRemaining ?? 0f;
        
        /// <summary>
        /// Gets the progress of the current phase (0.0 to 1.0).
        /// </summary>
        public float PhaseProgress => _currentState?.Progress ?? 0f;
        
        /// <summary>
        /// Gets whether the game loop is currently paused.
        /// </summary>
        public bool IsPaused => _isPaused;
        
        /// <summary>
        /// Gets whether the service is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Gets whether the game loop is currently running.
        /// </summary>
        public bool IsRunning => _isGameLoopActive;
        
        /// <summary>
        /// Gets the time remaining in the current phase (alias for PhaseTimeRemaining).
        /// </summary>
        public float TimeRemaining => PhaseTimeRemaining;
        
        /// <summary>
        /// Gets whether the game loop is currently transitioning between phases.
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
        
        /// <summary>
        /// Event fired when the game phase changes.
        /// Parameters: (previousPhase, newPhase)
        /// </summary>
        public event Action<GamePhase, GamePhase> OnPhaseChanged;
        
        /// <summary>
        /// Event fired when the phase time is updated.
        /// Parameter: timeRemaining (in seconds)
        /// </summary>
        public event Action<float> OnPhaseTimeUpdated;
        
        /// <summary>
        /// Event fired when the phase time is updated (with progress).
        /// Parameters: (timeRemaining, progress)
        /// </summary>
        public event Action<float, float> OnTimeUpdated;
        
        /// <summary>
        /// Unity Start method - automatically initializes the GameLoopManager.
        /// </summary>
        private void Start()
        {
            Initialize();
        }
        
        /// <summary>
        /// Initializes the game loop manager and sets up the phase states.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("GameLoopManager is already initialized.");
                return;
            }
            
            // Validate configuration FIRST before creating states
            if (config == null)
            {
                Debug.LogError("GameLoopConfig is not assigned! Creating default configuration.");
                config = CreateDefaultConfig();
            }
            
            // Initialize state dictionary
            _states = new Dictionary<GamePhase, GameLoopState>();
            
            // Create and initialize all phase states
            CreatePhaseStates();
            
            // Register this service with the ServiceLocator
            ServiceLocator.RegisterService<IGameLoopManager>(this);
            
            IsInitialized = true;

            if (enableDebugLogs)
            {
                Debug.Log("GameLoopManager initialized successfully.");
            }
        }
        
        /// <summary>
        /// Shuts down the game loop manager and cleans up resources.
        /// </summary>
        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            // Stop the game loop
            StopGameLoop();
            
            // Clear states
            _states?.Clear();
            _states = null;
            _currentState = null;
            
            // Note: Don't call UnregisterService here as it would cause recursion
            // ServiceLocator will handle cleanup when ShutdownAllServices is called
            
            IsInitialized = false;
            
            if (enableDebugLogs)
            {
                Debug.Log("GameLoopManager shut down successfully.");
            }
        }
        
        /// <summary>
        /// Starts the game loop from the Day phase.
        /// </summary>
        public void StartGameLoop()
        {
            if (!IsInitialized)
            {
                Debug.LogError("Cannot start game loop: GameLoopManager is not initialized.");
                return;
            }
            
            if (_isGameLoopActive)
            {
                Debug.LogWarning("Game loop is already active.");
                return;
            }
            
            _isGameLoopActive = true;
            _isPaused = false;
            
            // Start with the Day phase
            TransitionToPhase(GamePhase.Day);
            
            if (enableDebugLogs)
            {
                Debug.Log("Game loop started!");
            }
        }
        
        /// <summary>
        /// Stops the game loop.
        /// </summary>
        public void StopGameLoop()
        {
            if (!_isGameLoopActive)
            {
                return;
            }
            
            _isGameLoopActive = false;
            
            // Exit current state
            _currentState?.Exit();
            _currentState = null;
            
            if (enableDebugLogs)
            {
                Debug.Log("Game loop stopped.");
            }
        }
        
        /// <summary>
        /// Pauses the game loop timer.
        /// </summary>
        public void PauseGameLoop()
        {
            if (!_isGameLoopActive || _isPaused)
            {
                return;
            }
            
            _isPaused = true;
            _currentState?.Pause();
            
            if (enableDebugLogs)
            {
                Debug.Log("Game loop paused.");
            }
        }
        
        /// <summary>
        /// Resumes the game loop timer.
        /// </summary>
        public void ResumeGameLoop()
        {
            if (!_isGameLoopActive || !_isPaused)
            {
                return;
            }
            
            _isPaused = false;
            _currentState?.Resume();
            
            if (enableDebugLogs)
            {
                Debug.Log("Game loop resumed.");
            }
        }
        
        /// <summary>
        /// Alias for StartGameLoop() - for test compatibility.
        /// </summary>
        public void StartLoop() => StartGameLoop();
        
        /// <summary>
        /// Alias for StopGameLoop() - for test compatibility.
        /// </summary>
        public void StopLoop() => StopGameLoop();
        
        /// <summary>
        /// Alias for PauseGameLoop() - for test compatibility.
        /// </summary>
        public void Pause() => PauseGameLoop();
        
        /// <summary>
        /// Alias for ResumeGameLoop() - for test compatibility.
        /// </summary>
        public void Resume() => ResumeGameLoop();
        
        /// <summary>
        /// Forces an immediate transition to the specified phase.
        /// Use with caution - primarily for testing or special game events.
        /// </summary>
        /// <param name="targetPhase">The phase to transition to</param>
        public void ForcePhaseTransition(GamePhase targetPhase)
        {
            if (!IsInitialized)
            {
                Debug.LogError("Cannot force phase transition: GameLoopManager is not initialized.");
                return;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"Forcing transition to {targetPhase} phase.");
            }
            
            TransitionToPhase(targetPhase);
        }
        
        /// <summary>
        /// Transitions to the specified game phase.
        /// </summary>
        /// <param name="targetPhase">The phase to transition to</param>
        public void TransitionToPhase(GamePhase targetPhase)
        {
            if (!IsInitialized)
            {
                Debug.LogError("Cannot transition phase: GameLoopManager is not initialized.");
                return;
            }
            
            if (_isTransitioning)
            {
                Debug.LogWarning("Phase transition already in progress.");
                return;
            }
            
            if (!_states.TryGetValue(targetPhase, out var newState))
            {
                Debug.LogError($"No state found for phase {targetPhase}.");
                return;
            }
            
            if (_currentState == newState)
            {
                Debug.LogWarning($"Already in {targetPhase} phase.");
                return;
            }
            
            _isTransitioning = true;
            
            var previousPhase = _currentState?.Phase ?? GamePhase.Day;
            
            try
            {
                // Exit current state
                _currentState?.Exit();
                
                // Set new state
                _currentState = newState;
                
                // Enter new state
                _currentState.Enter();
                
                // Apply pause state if needed
                if (_isPaused)
                {
                    _currentState.Pause();
                }
                
                // Fire phase change event
                OnPhaseChanged?.Invoke(previousPhase, targetPhase);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"Phase changed from {previousPhase} to {targetPhase}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during phase transition: {ex.Message}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }
        
        /// <summary>
        /// Updates the current game loop state.
        /// Called by Unity's Update loop.
        /// </summary>
        private void Update()
        {
            if (!IsInitialized || !_isGameLoopActive || _isTransitioning || _isPaused)
            {
                return;
            }
            
            _currentState?.Update();
        }
        
        /// <summary>
        /// Fires time update events for the current phase.
        /// Called by GameLoopState during updates.
        /// </summary>
        /// <param name="timeRemaining">Time remaining in current phase</param>
        /// <param name="progress">Progress of current phase (0.0 to 1.0)</param>
        public void FireTimeUpdateEvents(float timeRemaining, float progress)
        {
            OnPhaseTimeUpdated?.Invoke(timeRemaining);
            OnTimeUpdated?.Invoke(timeRemaining, progress);
        }
        
        /// <summary>
        /// Creates and initializes all phase states.
        /// </summary>
        private void CreatePhaseStates()
        {
            // Create all phase states
            var dayState = new DayPhaseState();
            var buildState = new BuildPhaseState();
            var nightState = new NightPhaseState();
            var waveProgressState = new WaveProgressState();
            var endWaveState = new EndWaveState();
            
            // Initialize states
            dayState.Initialize(this, config);
            buildState.Initialize(this, config);
            nightState.Initialize(this, config);
            waveProgressState.Initialize(this, config);
            endWaveState.Initialize(this, config);
            
            // Add to dictionary
            _states[GamePhase.Day] = dayState;
            _states[GamePhase.Build] = buildState;
            _states[GamePhase.Night] = nightState;
            _states[GamePhase.WaveInProgress] = waveProgressState;
            _states[GamePhase.EndWave] = endWaveState;
            
            if (enableDebugLogs)
            {
                Debug.Log("All phase states created and initialized.");
            }
        }
        
        /// <summary>
        /// Creates a default configuration if none is assigned.
        /// </summary>
        /// <returns>Default GameLoopConfig</returns>
        private GameLoopConfig CreateDefaultConfig()
        {
            var defaultConfig = ScriptableObject.CreateInstance<GameLoopConfig>();
            
            // Set default values (these are already set in the ScriptableObject)
            // but we can override them here if needed
            
            return defaultConfig;
        }
        
        /// <summary>
        /// Gets the current phase state for external access.
        /// </summary>
        /// <typeparam name="T">The type of phase state to get</typeparam>
        /// <returns>The phase state instance, or null if not found</returns>
        public T GetPhaseState<T>() where T : GameLoopState
        {
            return _currentState as T;
        }
        
        /// <summary>
        /// Gets a specific phase state by phase type.
        /// </summary>
        /// <param name="phase">The phase to get the state for</param>
        /// <returns>The phase state instance, or null if not found</returns>
        public GameLoopState GetPhaseState(GamePhase phase)
        {
            return _states.TryGetValue(phase, out var state) ? state : null;
        }
        
        /// <summary>
        /// Cleanup when the GameObject is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
        }
        
        /// <summary>
        /// Validates the configuration in the editor.
        /// </summary>
        private void OnValidate()
        {
            if (config != null && Application.isPlaying && IsInitialized)
            {
                // Configuration changed during runtime - could reinitialize states if needed
                if (enableDebugLogs)
                {
                    Debug.Log("GameLoopConfig changed during runtime.");
                }
            }
        }
    }
}
