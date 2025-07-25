using Core.Time;
using Data;

namespace Core.GameLoop
{
    /// <summary>
    /// Base class for all game loop phase states.
    /// Provides common functionality and lifecycle methods for phase management.
    /// </summary>
    public abstract class GameLoopState
    {
        protected IGameLoopManager gameLoopManager;
        protected GameLoopConfig config;
        protected GameTimer timer;
        
        /// <summary>
        /// Gets the phase this state represents.
        /// </summary>
        public abstract GamePhase Phase { get; }
        
        /// <summary>
        /// Gets whether this state is currently active.
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// Gets the time remaining in this phase.
        /// </summary>
        public float TimeRemaining => timer?.TimeRemaining ?? 0f;
        
        /// <summary>
        /// Gets the progress of this phase (0.0 to 1.0).
        /// </summary>
        public float Progress => timer?.Progress ?? 0f;
        
        /// <summary>
        /// Initializes the game loop state with required dependencies.
        /// </summary>
        /// <param name="gameLoopManager">The game loop manager that owns this state</param>
        /// <param name="config">Configuration for timing and settings</param>
        public virtual void Initialize(IGameLoopManager gameLoopManager, GameLoopConfig config)
        {
            this.gameLoopManager = gameLoopManager;
            this.config = config;
            this.timer = new GameTimer();
        }
        
        /// <summary>
        /// Called when entering this phase state.
        /// Override to implement phase-specific entry logic.
        /// </summary>
        public virtual void Enter()
        {
            IsActive = true;
            
            // Start the timer for this phase
            if (config != null)
            {
                float phaseDuration = config.GetPhaseDuration(Phase);
                timer.Start(phaseDuration);
                UnityEngine.Debug.Log($"Entered {Phase} phase - Duration: {phaseDuration}s");
            }
            else
            {
                UnityEngine.Debug.LogError($"Cannot start {Phase} phase: GameLoopConfig is null!");
            }
        }
        
        /// <summary>
        /// Called every frame while this phase state is active.
        /// Override to implement phase-specific update logic.
        /// </summary>
        public virtual void Update()
        {
            if (!IsActive || timer == null) return;
            
            // Fire time update events
            if (gameLoopManager is GameLoopManager manager)
            {
                manager.FireTimeUpdateEvents(TimeRemaining, Progress);
            }
            
            // Check if phase should transition
            if (timer.IsComplete)
            {
                OnPhaseComplete();
            }
        }
        
        /// <summary>
        /// Called when exiting this phase state.
        /// Override to implement phase-specific cleanup logic.
        /// </summary>
        public virtual void Exit()
        {
            IsActive = false;
            timer?.Stop();
            
            UnityEngine.Debug.Log($"Exited {Phase} phase");
        }
        
        /// <summary>
        /// Pauses the phase timer.
        /// </summary>
        public virtual void Pause()
        {
            timer?.Pause();
        }
        
        /// <summary>
        /// Resumes the phase timer.
        /// </summary>
        public virtual void Resume()
        {
            timer?.Resume();
        }
        
        /// <summary>
        /// Called when the phase timer completes.
        /// Determines the next phase to transition to.
        /// </summary>
        protected virtual void OnPhaseComplete()
        {
            GamePhase nextPhase = GetNextPhase();
            
            if (gameLoopManager is GameLoopManager manager)
            {
                manager.TransitionToPhase(nextPhase);
            }
        }
        
        /// <summary>
        /// Gets the next phase in the game loop sequence.
        /// Override to implement custom phase progression logic.
        /// </summary>
        /// <returns>The next phase to transition to</returns>
        protected virtual GamePhase GetNextPhase()
        {
            return Phase switch
            {
                GamePhase.Day => GamePhase.Build,
                GamePhase.Build => GamePhase.Night,
                GamePhase.Night => GamePhase.WaveInProgress,
                GamePhase.WaveInProgress => GamePhase.EndWave,
                GamePhase.EndWave => GamePhase.Day, // Loop back to start
                _ => GamePhase.Day
            };
        }
        
        /// <summary>
        /// Gets the name of this phase state for debugging purposes.
        /// </summary>
        /// <returns>The name of the phase</returns>
        public virtual string GetStateName()
        {
            return Phase.ToString();
        }
    }
}
