# Game Loop and Day/Night Cycle System

This document describes the implementation of the core game loop state machine and day/night cycle system for the Outrage tower defense game.

## Overview

The game loop system manages the progression through different phases of gameplay:
- **Day Phase**: Resource collection and preparation
- **Build Phase**: Tower placement and strategic planning
- **Night Phase**: Final preparations before combat
- **Wave Progress**: Active combat with enemies
- **End Wave**: Post-combat upgrades and progression

The day/night cycle provides visual feedback and atmosphere that corresponds to the game phases.

## Core Components

### GameLoopManager

The main orchestrator of the game loop system.

**Key Features:**
- State machine managing phase transitions
- Configurable phase durations
- Pause/resume functionality
- Event system for phase changes and time updates
- Integration with ServiceLocator pattern

**Usage:**
```csharp
// Get the game loop manager
var gameLoop = ServiceLocator.GetService<IGameLoopManager>();

// Start the game loop
gameLoop.StartLoop();

// Listen for phase changes
gameLoop.OnPhaseChanged += (from, to) => {
    Debug.Log($"Phase changed from {from} to {to}");
};

// Force a phase transition
gameLoop.ForcePhaseTransition(GamePhase.Build);
```

### DayNightCycleManager

Manages visual transitions and lighting changes throughout the day/night cycle.

**Key Features:**
- Smooth time-of-day transitions
- Automatic lighting adjustments
- Sun light rotation and intensity
- Ambient color blending
- Integration with game phases

**Usage:**
```csharp
// Get the day/night manager
var dayNight = ServiceLocator.GetService<IDayNightCycle>();

// Set specific time of day
dayNight.SetTimeOfDay(0.5f); // Noon

// Smooth transition
dayNight.TransitionToTimeOfDay(0.75f, 2f); // Transition to dusk over 2 seconds

// Listen for day/night changes
dayNight.OnDayNightTransition += (isDay) => {
    Debug.Log(isDay ? "It's now day" : "It's now night");
};
```

### GameTimer

A utility class for precise, unscaled time calculations.

**Key Features:**
- Frame rate independent timing
- Pause/resume support
- Progress calculation
- Time manipulation methods

**Usage:**
```csharp
var timer = new GameTimer();
timer.Start(30f); // 30 second timer

// In Update loop
timer.Update(Time.deltaTime);

if (timer.IsComplete)
{
    Debug.Log("Timer finished!");
}
```

## Configuration

### GameLoopConfig ScriptableObject

Centralized configuration for all timing and visual settings.

**Properties:**
- `dayPhaseDuration`: Duration of the day phase in seconds
- `buildPhaseDuration`: Duration of the build phase in seconds
- `nightPhaseDuration`: Duration of the night phase in seconds
- `waveProgressDuration`: Duration of the wave progress phase in seconds
- `endWaveDuration`: Duration of the end wave phase in seconds
- `dayAmbientColor`: Ambient light color during day
- `nightAmbientColor`: Ambient light color during night
- `lightTransitionCurve`: Animation curve for lighting transitions

**Creating a Config:**
1. Right-click in Project window
2. Create > Game Data > Game Loop Config
3. Configure the timing and visual settings
4. Assign to GameLoopManager and DayNightCycleManager

## Scene Setup

### Basic Setup

1. **Create GameLoopManager:**
   - Create empty GameObject named "GameLoopManager"
   - Add `GameLoopManager` component
   - Assign `GameLoopConfig` asset

2. **Create DayNightCycleManager:**
   - Create empty GameObject named "DayNightCycleManager"
   - Add `DayNightCycleManager` component
   - Assign `GameLoopConfig` asset
   - Assign directional light for sun

3. **Initialize in GameManager:**
   - The `GameManager` automatically initializes both managers
   - Ensure GameManager is present in scene

### Using GameLoopExample

For testing and demonstration:

1. Add `GameLoopExample` component to any GameObject
2. Assign `GameLoopConfig` and sun light
3. Enable `autoStartLoop` for automatic startup
4. Enable `showDebugUI` for runtime controls

## Integration Points

### Service Integration

Both managers register with the ServiceLocator:

```csharp
// Access from anywhere in the codebase
var gameLoop = ServiceLocator.GetService<IGameLoopManager>();
var dayNight = ServiceLocator.GetService<IDayNightCycle>();
```

### Event Communication

Subscribe to events for system integration:

```csharp
// Game loop events
gameLoop.OnPhaseChanged += HandlePhaseChange;
gameLoop.OnTimeUpdated += HandleTimeUpdate;

// Day/night events
dayNight.OnTimeOfDayChanged += HandleTimeOfDayChange;
dayNight.OnDayNightTransition += HandleDayNightTransition;
```

### UI Integration

Example UI integration:

```csharp
public class GameLoopUI : MonoBehaviour
{
    [SerializeField] private Text phaseText;
    [SerializeField] private Slider timeSlider;
    
    private IGameLoopManager _gameLoop;
    
    private void Start()
    {
        _gameLoop = ServiceLocator.GetService<IGameLoopManager>();
        _gameLoop.OnPhaseChanged += UpdatePhaseDisplay;
        _gameLoop.OnTimeUpdated += UpdateTimeDisplay;
    }
    
    private void UpdatePhaseDisplay(GamePhase from, GamePhase to)
    {
        phaseText.text = to.ToString();
    }
    
    private void UpdateTimeDisplay(float timeRemaining, float progress)
    {
        timeSlider.value = progress;
    }
}
```

## Testing

### Unit Tests

- `GameLoopManagerTests`: Tests core game loop functionality
- `GameTimerTests`: Tests timer utility class

### Integration Tests

- `DayNightCycleIntegrationTests`: Tests visual transitions and lighting

### Running Tests

1. Open Window > General > Test Runner
2. Select EditMode or PlayMode tab
3. Run individual tests or entire suites

## Performance Considerations

### Optimization Tips

1. **Lighting Updates**: Day/night transitions use smooth interpolation to avoid frame rate spikes
2. **Event Frequency**: Time update events are throttled to reasonable intervals
3. **State Caching**: Game loop states are cached to avoid repeated instantiation
4. **Unscaled Time**: GameTimer uses unscaled time for consistent behavior

### Memory Management

- Managers properly clean up resources on shutdown
- Event subscriptions are removed in OnDestroy
- ScriptableObject configs are reused across scenes

## Troubleshooting

### Common Issues

1. **Game loop not starting:**
   - Ensure GameManager is initialized
   - Check GameLoopConfig is assigned
   - Verify ServiceLocator registration

2. **Lighting not updating:**
   - Assign sun light to DayNightCycleManager
   - Check GameLoopConfig has valid colors/curves
   - Ensure manager is initialized

3. **Events not firing:**
   - Verify event subscriptions are set up correctly
   - Check managers are running and not paused
   - Ensure proper cleanup in OnDestroy

### Debug Tools

- Use `GameLoopExample` component for runtime debugging
- Enable debug logs in manager components
- Check Unity Console for initialization messages
- Use Test Runner for automated validation

## Future Enhancements

### Planned Features

- Weather system integration
- Seasonal variations
- Dynamic phase duration based on difficulty
- Audio integration for ambient sounds
- Visual effects for phase transitions

### Extension Points

- Custom game loop states
- Additional lighting effects
- Integration with other game systems
- Save/load support for game loop state
- Multiplayer synchronization