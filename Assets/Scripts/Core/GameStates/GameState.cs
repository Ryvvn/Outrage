using Core.Services;

namespace Core.GameStates
{
    /// <summary>
    /// Base class for all game states in the state machine.
    /// Provides common functionality and lifecycle methods for state management.
    /// </summary>
    public abstract class GameState
    {
        protected IGameManager gameManager;
        
        /// <summary>
        /// Gets whether this state is currently active.
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// Initializes the game state with a reference to the game manager.
        /// </summary>
        /// <param name="gameManager">The game manager that owns this state.</param>
        public virtual void Initialize(IGameManager gameManager)
        {
            this.gameManager = gameManager;
        }
        
        /// <summary>
        /// Called when entering this state.
        /// Override to implement state-specific entry logic.
        /// </summary>
        public virtual void Enter()
        {
            IsActive = true;
        }
        
        /// <summary>
        /// Called every frame while this state is active.
        /// Override to implement state-specific update logic.
        /// </summary>
        public virtual void Update()
        {
            // Base implementation does nothing
        }
        
        /// <summary>
        /// Called when exiting this state.
        /// Override to implement state-specific cleanup logic.
        /// </summary>
        public virtual void Exit()
        {
            IsActive = false;
        }
        
        /// <summary>
        /// Gets the name of this state for debugging purposes.
        /// </summary>
        /// <returns>The name of the state class.</returns>
        public virtual string GetStateName()
        {
            return GetType().Name;
        }
    }
}

