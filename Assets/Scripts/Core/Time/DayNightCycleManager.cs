using System;
using System.Collections;
using UnityEngine;
using Core.Services;
using Core.GameLoop;
using Data;

namespace Core.Time
{
    /// <summary>
    /// Manages day/night visual transitions and lighting changes.
    /// Integrates with the game loop to provide visual feedback for different phases.
    /// </summary>
    public class DayNightCycleManager : MonoBehaviour, IDayNightCycle
    {
        [Header("Configuration")]
        [SerializeField] private GameLoopConfig config;
        
        [Header("Lighting Settings")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Gradient sunColorGradient;
        [SerializeField] private AnimationCurve sunIntensityCurve;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool enableSmoothTransitions = true;
        
        private float _currentTimeOfDay;
        private bool _isPaused;
        private bool _isTransitioning;
        private Coroutine _transitionCoroutine;
        
        /// <summary>
        /// Gets the current day progress (0.0 = start of day, 1.0 = end of day).
        /// </summary>
        public float DayProgress
        {
            get
            {
                // Map time of day to day progress
                // 0.25 (dawn) = 0.0, 0.75 (dusk) = 1.0
                if (_currentTimeOfDay < 0.25f) return 0f;
                if (_currentTimeOfDay > 0.75f) return 1f;
                return (_currentTimeOfDay - 0.25f) / 0.5f;
            }
        }
        
        /// <summary>
        /// Gets whether it is currently day time.
        /// </summary>
        public bool IsDay => _currentTimeOfDay >= 0.25f && _currentTimeOfDay <= 0.75f;
        
        /// <summary>
        /// Gets the current ambient light color.
        /// </summary>
        public Color CurrentAmbientColor => RenderSettings.ambientLight;
        
        /// <summary>
        /// Gets the current time of day as a normalized value (0.0 to 1.0).
        /// </summary>
        public float TimeOfDay => _currentTimeOfDay;
        
        /// <summary>
        /// Gets whether the service is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Event fired when the time of day changes.
        /// </summary>
        public event Action<float> OnTimeOfDayChanged;
        
        /// <summary>
        /// Event fired when transitioning between day and night.
        /// </summary>
        public event Action<bool> OnDayNightTransition;
        
        /// <summary>
        /// Unity Start method - automatically initializes the DayNightCycleManager.
        /// </summary>
        private void Start()
        {
            Initialize();
        }
        
        /// <summary>
        /// Initializes the day/night cycle manager.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("DayNightCycleManager is already initialized.");
                return;
            }
            
            // Validate configuration
            if (config == null)
            {
                Debug.LogError("GameLoopConfig is not assigned to DayNightCycleManager!");
            }
            
            // Initialize default values
            _currentTimeOfDay = 0.25f; // Start at dawn
            _isPaused = false;
            _isTransitioning = false;
            
            // Setup default gradients if not assigned
            SetupDefaultGradients();
            
            // Find sun light if not assigned
            if (sunLight == null)
            {
                sunLight = FindObjectOfType<Light>();
                if (sunLight != null && enableDebugLogs)
                {
                    Debug.Log("Sun light automatically found and assigned.");
                }
            }
            
            // Apply initial lighting
            UpdateLighting();
            
            // Register this service with the ServiceLocator
            ServiceLocator.RegisterService<IDayNightCycle>(this);
            
            IsInitialized = true;
            
            if (enableDebugLogs)
            {
                Debug.Log("DayNightCycleManager initialized successfully.");
            }
        }
        
        /// <summary>
        /// Shuts down the day/night cycle manager.
        /// </summary>
        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            // Stop any running transitions
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }
            
            // Note: Don't call UnregisterService here as it would cause recursion
            // ServiceLocator will handle cleanup when ShutdownAllServices is called
            
            IsInitialized = false;
            
            if (enableDebugLogs)
            {
                Debug.Log("DayNightCycleManager shut down successfully.");
            }
        }
        
        /// <summary>
        /// Sets the time of day to a specific normalized value.
        /// </summary>
        /// <param name="normalizedTime">Time of day (0.0 to 1.0)</param>
        public void SetTimeOfDay(float normalizedTime)
        {
            if (!IsInitialized)
            {
                return;
            }
            
            bool wasDay = IsDay;
            _currentTimeOfDay = Mathf.Clamp01(normalizedTime);
            
            UpdateLighting();
            
            // Fire events
            OnTimeOfDayChanged?.Invoke(_currentTimeOfDay);
            
            if (wasDay != IsDay)
            {
                OnDayNightTransition?.Invoke(IsDay);
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"Time of day set to {_currentTimeOfDay:F2} ({(IsDay ? "Day" : "Night")})");
            }
        }
        
        /// <summary>
        /// Smoothly transitions to a specific time of day over a duration.
        /// </summary>
        /// <param name="targetTime">Target time of day (0.0 to 1.0)</param>
        /// <param name="duration">Duration of the transition in seconds</param>
        public void TransitionToTimeOfDay(float targetTime, float duration)
        {
            if (!IsInitialized)
            {
                return;
            }
            
            if (!enableSmoothTransitions || duration <= 0f)
            {
                SetTimeOfDay(targetTime);
                return;
            }
            
            // Stop any existing transition
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            _transitionCoroutine = StartCoroutine(TransitionCoroutine(targetTime, duration));
        }
        
        /// <summary>
        /// Updates the day/night cycle based on the current game phase.
        /// </summary>
        /// <param name="phase">Current game phase</param>
        /// <param name="phaseProgress">Progress within the current phase (0.0 to 1.0)</param>
        public void UpdateForGamePhase(GamePhase phase, float phaseProgress)
        {
            if (!IsInitialized || _isPaused)
            {
                return;
            }
            
            float targetTimeOfDay = GetTimeOfDayForPhase(phase, phaseProgress);
            
            if (enableSmoothTransitions && !_isTransitioning)
            {
                // Smooth transition during phase
                float transitionSpeed = 0.5f; // Adjust as needed
                _currentTimeOfDay = Mathf.MoveTowards(_currentTimeOfDay, targetTimeOfDay, 
                                                    transitionSpeed * UnityEngine.Time.deltaTime);
            }
            else
            {
                _currentTimeOfDay = targetTimeOfDay;
            }
            
            UpdateLighting();
            OnTimeOfDayChanged?.Invoke(_currentTimeOfDay);
        }
        
        /// <summary>
        /// Pauses the day/night cycle progression.
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
            
            if (enableDebugLogs)
            {
                Debug.Log("Day/night cycle paused.");
            }
        }
        
        /// <summary>
        /// Resumes the day/night cycle progression.
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
            
            if (enableDebugLogs)
            {
                Debug.Log("Day/night cycle resumed.");
            }
        }
        
        /// <summary>
        /// Gets the appropriate time of day for a given game phase and progress.
        /// </summary>
        /// <param name="phase">The game phase</param>
        /// <param name="phaseProgress">Progress within the phase (0.0 to 1.0)</param>
        /// <returns>Time of day (0.0 to 1.0)</returns>
        private float GetTimeOfDayForPhase(GamePhase phase, float phaseProgress)
        {
            return phase switch
            {
                GamePhase.Day => Mathf.Lerp(0.25f, 0.45f, phaseProgress), // Dawn to mid-morning
                GamePhase.Build => Mathf.Lerp(0.45f, 0.65f, phaseProgress), // Mid-morning to afternoon
                GamePhase.Night => Mathf.Lerp(0.65f, 0.85f, phaseProgress), // Afternoon to night
                GamePhase.WaveInProgress => Mathf.Lerp(0.85f, 0.95f, phaseProgress), // Night to deep night
                GamePhase.EndWave => Mathf.Lerp(0.95f, 0.25f, phaseProgress), // Deep night back to dawn
                _ => 0.25f // Default to dawn
            };
        }
        
        /// <summary>
        /// Updates the lighting based on the current time of day.
        /// </summary>
        private void UpdateLighting()
        {
            if (config == null) return;
            
            // Update ambient lighting
            Color ambientColor = Color.Lerp(config.nightAmbientColor, config.dayAmbientColor, 
                                          GetDayNightBlend());
            RenderSettings.ambientLight = ambientColor;
            
            // Update sun light if available
            if (sunLight != null)
            {
                UpdateSunLight();
            }
        }
        
        /// <summary>
        /// Updates the sun light properties based on time of day.
        /// </summary>
        private void UpdateSunLight()
        {
            if (sunLight == null) return;
            
            // Update sun color
            if (sunColorGradient != null)
            {
                sunLight.color = sunColorGradient.Evaluate(_currentTimeOfDay);
            }
            
            // Update sun intensity
            if (sunIntensityCurve != null)
            {
                sunLight.intensity = sunIntensityCurve.Evaluate(_currentTimeOfDay);
            }
            
            // Update sun rotation (simple east to west movement)
            float sunAngle = (_currentTimeOfDay - 0.25f) * 180f; // -45° to 135°
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 30f, 0f);
        }
        
        /// <summary>
        /// Gets the day/night blend factor for lighting calculations.
        /// </summary>
        /// <returns>Blend factor (0.0 = night, 1.0 = day)</returns>
        private float GetDayNightBlend()
        {
            if (config?.lightTransitionCurve != null)
            {
                return config.lightTransitionCurve.Evaluate(DayProgress);
            }
            
            return DayProgress;
        }
        
        /// <summary>
        /// Sets up default gradients if they are not assigned.
        /// </summary>
        private void SetupDefaultGradients()
        {
            if (sunColorGradient == null)
            {
                sunColorGradient = new Gradient();
                var colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0f), // Midnight - dark blue
                    new GradientColorKey(new Color(1f, 0.6f, 0.3f), 0.25f), // Dawn - orange
                    new GradientColorKey(Color.white, 0.5f), // Noon - white
                    new GradientColorKey(new Color(1f, 0.4f, 0.2f), 0.75f), // Dusk - red
                    new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 1f) // Midnight - dark blue
                };
                
                var alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                };
                
                sunColorGradient.SetKeys(colorKeys, alphaKeys);
            }
            
            if (sunIntensityCurve == null)
            {
                sunIntensityCurve = new AnimationCurve();
                sunIntensityCurve.AddKey(0f, 0f); // Midnight - no light
                sunIntensityCurve.AddKey(0.25f, 0.5f); // Dawn - dim
                sunIntensityCurve.AddKey(0.5f, 1f); // Noon - bright
                sunIntensityCurve.AddKey(0.75f, 0.5f); // Dusk - dim
                sunIntensityCurve.AddKey(1f, 0f); // Midnight - no light
            }
        }
        
        /// <summary>
        /// Coroutine for smooth time of day transitions.
        /// </summary>
        /// <param name="targetTime">Target time of day</param>
        /// <param name="duration">Transition duration</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator TransitionCoroutine(float targetTime, float duration)
        {
            _isTransitioning = true;
            
            float startTime = _currentTimeOfDay;
            float elapsedTime = 0f;
            
            bool wasDay = IsDay;
            
            while (elapsedTime < duration)
            {
                if (_isPaused)
                {
                    yield return null;
                    continue;
                }
                
                elapsedTime += UnityEngine.Time.deltaTime;
                float progress = elapsedTime / duration;
                
                // Apply easing curve if available
                if (config?.lightTransitionCurve != null)
                {
                    progress = config.lightTransitionCurve.Evaluate(progress);
                }
                
                _currentTimeOfDay = Mathf.Lerp(startTime, targetTime, progress);
                UpdateLighting();
                
                OnTimeOfDayChanged?.Invoke(_currentTimeOfDay);
                
                // Check for day/night transition
                if (wasDay != IsDay)
                {
                    OnDayNightTransition?.Invoke(IsDay);
                    wasDay = IsDay;
                }
                
                yield return null;
            }
            
            // Ensure we reach the exact target
            _currentTimeOfDay = targetTime;
            UpdateLighting();
            OnTimeOfDayChanged?.Invoke(_currentTimeOfDay);
            
            _isTransitioning = false;
            _transitionCoroutine = null;
            
            if (enableDebugLogs)
            {
                Debug.Log($"Time of day transition completed: {targetTime:F2}");
            }
        }
        
        /// <summary>
        /// Cleanup when the GameObject is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
