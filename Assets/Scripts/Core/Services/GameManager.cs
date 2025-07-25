using System;
using UnityEngine;
using Core.GameStates;

namespace Core.Services
{
    /// <summary>
    /// Core game manager that handles game state transitions and overall game flow.
    /// Implements the IGameManager interface and manages the game state machine.
    /// </summary>
    public class GameManager : MonoBehaviour, IGameManager
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private GameState _currentState;
        private bool _isTransitioning;
        
        /// <summary>
        /// Gets the currently active game state.
        /// </summary>
        public GameState CurrentState => _currentState;
        
        /// <summary>
        /// Gets whether the service is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Gets whether the game manager is currently transitioning between states.
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
        
        /// <summary>
        /// Event fired when the game state changes.
        /// Parameters: (previousState, newState)
        /// </summary>
        public event Action<GameState, GameState> OnStateChanged;
        
        /// <summary>
        /// Initializes the game manager and registers it with the service locator.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("GameManager is already initialized.");
                return;
            }
            
            // Register this service with the ServiceLocator
            ServiceLocator.RegisterService<IGameManager>(this);
            
            IsInitialized = true;
            
            if (enableDebugLogs)
            {
                Debug.Log("GameManager initialized successfully.");
            }
        }
        
        /// <summary>
        /// Shuts down the game manager and cleans up resources.
        /// </summary>
        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            // Exit current state
            if (_currentState != null)
            {
                _currentState.Exit();
                _currentState = null;
            }
            
            IsInitialized = false;
            
            // Note: Don't call UnregisterService here as it would cause recursion
            // ServiceLocator will handle cleanup when ShutdownAllServices is called
            
            if (enableDebugLogs)
            {
                Debug.Log("GameManager shut down successfully.");
            }
        }
        
        /// <summary>
        /// Changes to a new game state of the specified type.
        /// Creates a new instance if one doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of state to transition to.</typeparam>
        public void ChangeState<T>() where T : GameState, new()
        {
            var newState = new T();
            ChangeState(newState);
        }
        
        /// <summary>
        /// Changes to a specific game state instance.
        /// </summary>
        /// <param name="newState">The state instance to transition to.</param>
        public void ChangeState(GameState newState)
        {
            if (!IsInitialized)
            {
                Debug.LogError("Cannot change state: GameManager is not initialized.");
                return;
            }
            
            if (_isTransitioning)
            {
                Debug.LogWarning("State transition already in progress. Ignoring new transition request.");
                return;
            }
            
            if (newState == null)
            {
                Debug.LogError("Cannot transition to null state.");
                return;
            }
            
            _isTransitioning = true;
            
            var previousState = _currentState;
            
            try
            {
                // Exit current state
                if (_currentState != null)
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"Exiting state: {_currentState.GetType().Name}");
                    }
                    _currentState.Exit();
                }
                
                // Set new state
                _currentState = newState;
                _currentState.Initialize(this);
                
                // Enter new state
                if (enableDebugLogs)
                {
                    Debug.Log($"Entering state: {_currentState.GetType().Name}");
                }
                _currentState.Enter();
                
                // Fire state change event
                OnStateChanged?.Invoke(previousState, _currentState);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during state transition: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }
        
        /// <summary>
        /// Updates the current game state.
        /// Called by Unity's Update loop.
        /// </summary>
        private void Update()
        {
            if (!IsInitialized || _isTransitioning)
            {
                return;
            }
            
            // Update current state
            _currentState?.Update();
        }
        
        /// <summary>
        /// Handles application quit cleanup.
        /// </summary>
        private void OnApplicationQuit()
        {
            if (IsInitialized)
            {
                Shutdown();
            }
        }
        
        /// <summary>
        /// Handles component destruction cleanup.
        /// </summary>
        private void OnDestroy()
        {
            if (IsInitialized)
            {
                Shutdown();
            }
        }
    }
}
