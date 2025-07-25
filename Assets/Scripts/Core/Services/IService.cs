using System;

namespace Core.Services
{
    /// <summary>
    /// Base interface for all services in the game.
    /// Provides common lifecycle methods for service management.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Initialize the service. Called during game startup.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Shutdown the service. Called during game shutdown or service cleanup.
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Gets whether the service has been initialized and is ready for use.
        /// </summary>
        bool IsInitialized { get; }
    }
}
