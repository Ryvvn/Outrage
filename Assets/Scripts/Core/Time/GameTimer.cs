using UnityEngine;

namespace Core.Time
{
    /// <summary>
    /// Precise game timing system that maintains accuracy across frame rate variations.
    /// Uses unscaled time to be resistant to game pause states.
    /// </summary>
    public class GameTimer
    {
        private float _duration;
        private float _startTime;
        private bool _isRunning;
        private bool _isPaused;
        private float _pausedTime;
        
        /// <summary>
        /// Gets whether the timer is currently running.
        /// </summary>
        public bool IsRunning => _isRunning && !_isPaused;
        
        /// <summary>
        /// Gets whether the timer is paused.
        /// </summary>
        public bool IsPaused => _isPaused;
        
        /// <summary>
        /// Gets the total duration of the timer.
        /// </summary>
        public float Duration => _duration;
        
        /// <summary>
        /// Gets the elapsed time since the timer started.
        /// </summary>
        public float ElapsedTime
        {
            get
            {
                if (!_isRunning) return 0f;
                if (_isPaused) return _pausedTime;
                return UnityEngine.Time.unscaledTime - _startTime;
            }
        }
        
        /// <summary>
        /// Gets the remaining time on the timer.
        /// </summary>
        public float TimeRemaining
        {
            get
            {
                if (!_isRunning) return 0f;
                return Mathf.Max(0f, _duration - ElapsedTime);
            }
        }
        
        /// <summary>
        /// Gets the remaining time on the timer (alias for test compatibility).
        /// </summary>
        public float RemainingTime => TimeRemaining;
        
        /// <summary>
        /// Gets the progress of the timer (0.0 to 1.0).
        /// </summary>
        public float Progress
        {
            get
            {
                if (!_isRunning || _duration <= 0f) return 0f;
                return Mathf.Clamp01(ElapsedTime / _duration);
            }
        }
        
        /// <summary>
        /// Gets whether the timer has completed.
        /// </summary>
        public bool IsComplete => _isRunning && ElapsedTime >= _duration;
        
        /// <summary>
        /// Starts the timer with the specified duration.
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        public void Start(float duration)
        {
            _duration = duration;
            _startTime = UnityEngine.Time.unscaledTime;
            _isRunning = true;
            _isPaused = false;
            _pausedTime = 0f;
        }
        
        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
            _pausedTime = 0f;
        }
        
        /// <summary>
        /// Pauses the timer, preserving the current elapsed time.
        /// </summary>
        public void Pause()
        {
            if (!_isRunning || _isPaused) return;
            
            _pausedTime = ElapsedTime;
            _isPaused = true;
        }
        
        /// <summary>
        /// Resumes the timer from where it was paused.
        /// </summary>
        public void Resume()
        {
            if (!_isRunning || !_isPaused) return;
            
            _startTime = UnityEngine.Time.unscaledTime - _pausedTime;
            _isPaused = false;
            _pausedTime = 0f;
        }
        
        /// <summary>
        /// Resets the timer to its initial state.
        /// </summary>
        public void Reset()
        {
            Stop();
        }
        
        /// <summary>
        /// Adds time to the current timer duration.
        /// </summary>
        /// <param name="additionalTime">Time to add in seconds</param>
        public void AddTime(float additionalTime)
        {
            _duration += additionalTime;
        }
        
        /// <summary>
        /// Sets the timer to a specific remaining time.
        /// </summary>
        /// <param name="timeRemaining">Time remaining in seconds</param>
        public void SetTimeRemaining(float timeRemaining)
        {
            if (!_isRunning) return;
            
            float newElapsedTime = _duration - timeRemaining;
            _startTime = UnityEngine.Time.unscaledTime - newElapsedTime;
            
            if (_isPaused)
            {
                _pausedTime = newElapsedTime;
            }
        }
        
        /// <summary>
        /// Sets the timer to a specific remaining time (alias for test compatibility).
        /// </summary>
        /// <param name="remainingTime">Time remaining in seconds</param>
        public void SetRemainingTime(float remainingTime) => SetTimeRemaining(remainingTime);
        
        /// <summary>
        /// Updates the timer (for compatibility with tests).
        /// This method doesn't need to do anything as the timer is self-updating.
        /// </summary>
        public void Update()
        {
            // Timer is self-updating based on Unity's time system
            // This method exists for test compatibility
        }
        
        /// <summary>
        /// Updates the timer with manual delta time (for testing).
        /// </summary>
        /// <param name="deltaTime">Time to advance in seconds</param>
        public void Update(float deltaTime)
        {
            if (!_isRunning || _isPaused || deltaTime <= 0f) return;
            
            // Manually advance the timer by adjusting the start time
            _startTime -= deltaTime;
            
            // Check if timer should complete
            if (ElapsedTime >= _duration)
            {
                _isRunning = false;
            }
        }
    }
}
