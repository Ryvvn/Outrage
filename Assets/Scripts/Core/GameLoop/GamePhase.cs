namespace Core.GameLoop
{
    /// <summary>
    /// Defines the main phases of the game loop.
    /// Represents the core "Day → Build → Night → Survive → Upgrade" gameplay cycle.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// Day phase - Resource collection and preparation activities
        /// </summary>
        Day,
        
        /// <summary>
        /// Build phase - Tower placement and strategic planning
        /// </summary>
        Build,
        
        /// <summary>
        /// Night phase - Preparation for incoming wave
        /// </summary>
        Night,
        
        /// <summary>
        /// Wave in progress - Combat and survival
        /// </summary>
        WaveInProgress,
        
        /// <summary>
        /// End wave - Upgrade choices and progression
        /// </summary>
        EndWave
    }
}
