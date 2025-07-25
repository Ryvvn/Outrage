using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.GameLoop;
using Core.Services;
using Core.Time;

namespace UI
{
    /// <summary>
    /// UI controller for displaying game loop information.
    /// Shows current phase, timer, and day/night cycle progress.
    /// </summary>
    public class GameLoopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Slider phaseProgressSlider;
        [SerializeField] private TextMeshProUGUI dayNightText;
        [SerializeField] private Image backgroundImage;
        
        [Header("Visual Settings")]
        [SerializeField] private Color dayBackgroundColor = new Color(0.8f, 0.9f, 1f, 0.3f);
        [SerializeField] private Color nightBackgroundColor = new Color(0.2f, 0.2f, 0.4f, 0.3f);
        
        private IGameLoopManager _gameLoopManager;
        private IDayNightCycle _dayNightCycle;
        private bool _isInitialized = false;
        private GameObject _createdGameLoopManagerObj; // Track created GameObject for cleanup
        
        private void Start()
        {
            // Wait a frame to ensure services are initialized
            Invoke(nameof(InitializeUI), 0.1f);
        }
        
        private void InitializeUI()
        {
            try
            {
                // Get services from ServiceLocator
                _gameLoopManager = ServiceLocator.GetService<IGameLoopManager>();
                if (_gameLoopManager == null)
                {
                    Debug.LogWarning("GameLoopManager service not found. GameLoopUI will not function properly. Ensure GameBootstrap has initialized the GameLoopManager service.");
                    return;
                }

                // Similarly, ensure the DayNightCycle service is available
                _dayNightCycle = ServiceLocator.GetService<IDayNightCycle>();
                if (_dayNightCycle == null)
                {
                    Debug.LogWarning("DayNightCycle service not found. Some functionality will be limited.");
                }
                if (_gameLoopManager != null)
                {
                    // Subscribe to game loop events
                    _gameLoopManager.OnPhaseChanged += OnPhaseChanged;
                    _gameLoopManager.OnPhaseTimeUpdated += OnPhaseTimeUpdated;
                    
                    _isInitialized = true;
                    Debug.Log("GameLoopUI initialized successfully");
                    
                    // Update UI with current state
                    UpdatePhaseDisplay(_gameLoopManager.CurrentPhase);
                    UpdateTimerDisplay(_gameLoopManager.PhaseTimeRemaining, _gameLoopManager.PhaseProgress);
                }
                else
                {
                    Debug.LogWarning("GameLoopManager service not found. UI will not function.");
                }
                
                if (_dayNightCycle != null)
                {
                    _dayNightCycle.OnTimeOfDayChanged += OnTimeOfDayChanged;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize GameLoopUI: {ex.Message}");
            }
        }
        
        private void Update()
        {
            if (!_isInitialized || _gameLoopManager == null)
                return;
                
            // Update timer display every frame for smooth countdown
            UpdateTimerDisplay(_gameLoopManager.PhaseTimeRemaining, _gameLoopManager.PhaseProgress);
        }
        
        private void OnPhaseChanged(GamePhase previousPhase, GamePhase newPhase)
        {
            UpdatePhaseDisplay(newPhase);
            Debug.Log($"Phase changed from {previousPhase} to {newPhase}");
        }
        
        private void OnPhaseTimeUpdated(float timeRemaining)
        {
            // This is handled in Update() for smoother display
        }
        
        private void OnTimeOfDayChanged(float timeOfDay)
        {
            if (_dayNightCycle != null)
            {
                UpdateDayNightDisplay();
                UpdateBackgroundColor(timeOfDay);
            }
        }
        
        private void UpdatePhaseDisplay(GamePhase phase)
        {
            if (phaseText != null)
            {
                string phaseDisplayName = phase switch
                {
                    GamePhase.Day => "DAY - Gather Resources",
                    GamePhase.Build => "BUILD - Place Towers",
                    GamePhase.Night => "NIGHT - Prepare for Battle",
                    GamePhase.WaveInProgress => "COMBAT - Survive the Wave",
                    GamePhase.EndWave => "UPGRADE - Choose Improvements",
                    _ => phase.ToString()
                };
                
                phaseText.text = phaseDisplayName;
                
                // Set color based on phase
                Color phaseColor = phase switch
                {
                    GamePhase.Day => Color.yellow,
                    GamePhase.Build => Color.green,
                    GamePhase.Night => Color.blue,
                    GamePhase.WaveInProgress => Color.red,
                    GamePhase.EndWave => Color.magenta,
                    _ => Color.white
                };
                
                phaseText.color = phaseColor;
            }
        }
        
        private void UpdateTimerDisplay(float timeRemaining, float progress)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
            
            if (phaseProgressSlider != null)
            {
                phaseProgressSlider.value = progress;
            }
        }
        
        private void UpdateDayNightDisplay()
        {
            if (dayNightText != null && _dayNightCycle != null)
            {
                string timeOfDayText = _dayNightCycle.IsDay ? "DAY" : "NIGHT";
                float dayProgress = _dayNightCycle.DayProgress;
                dayNightText.text = $"{timeOfDayText} ({dayProgress:P0})";
            }
        }
        
        private void UpdateBackgroundColor(float timeOfDay)
        {
            if (backgroundImage != null)
            {
                // Interpolate between day and night colors based on time of day
                // 0.25 = dawn, 0.75 = dusk
                float dayNightFactor;
                if (timeOfDay < 0.25f || timeOfDay > 0.75f)
                {
                    // Night time
                    dayNightFactor = 0f;
                }
                else
                {
                    // Day time
                    dayNightFactor = 1f;
                }
                
                Color targetColor = Color.Lerp(nightBackgroundColor, dayBackgroundColor, dayNightFactor);
                backgroundImage.color = targetColor;
            }
        }
        
        private void OnDestroy()
        {
            // Cancel any pending Invoke calls
            CancelInvoke();
            
            // Unsubscribe from events to prevent memory leaks
            try
            {
                if (_gameLoopManager != null)
                {
                    _gameLoopManager.OnPhaseChanged -= OnPhaseChanged;
                    _gameLoopManager.OnPhaseTimeUpdated -= OnPhaseTimeUpdated;
                }
                
                if (_dayNightCycle != null)
                {
                    _dayNightCycle.OnTimeOfDayChanged -= OnTimeOfDayChanged;
                }
            }
            catch (System.Exception ex)
            {
                // Ignore errors during shutdown - services might already be destroyed
                Debug.LogWarning($"GameLoopUI cleanup warning: {ex.Message}");
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
                    Debug.LogWarning($"GameLoopUI GameObject cleanup warning: {ex.Message}");
                }
                finally
                {
                    _createdGameLoopManagerObj = null;
                }
            }
        }
    }
}