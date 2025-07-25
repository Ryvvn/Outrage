# Procedural Tower Defense Game

A Unity-based tower defense game featuring procedural generation, dynamic difficulty scaling, and strategic gameplay.

## Project Structure

### Core Architecture

The project follows a service-oriented architecture with a state machine pattern for game flow management.

#### Key Components

- **Service Locator Pattern**: Centralized service management and dependency injection
- **Game State Machine**: Manages different game states (Initialization, Menu, Gameplay)
- **Bootstrap System**: Handles game initialization and service setup

#### Directory Structure

```
Assets/
├── Scripts/
│   └── Core/
│       ├── Services/           # Core service interfaces and implementations
│       ├── GameStates/         # Game state implementations
│       └── Bootstrap/          # Game initialization
├── Scenes/
│   ├── Bootstrap.unity         # Initial bootstrap scene
│   ├── MainMenu.unity          # Main menu scene
│   └── Gameplay.unity          # Core gameplay scene
└── Tests/
    ├── EditMode/               # Unit tests
    └── PlayMode/               # Integration tests
```

## Core Systems

### Service Locator

The `ServiceLocator` provides centralized service management:

```csharp
// Register a service
ServiceLocator.RegisterService<IGameManager>(gameManager);

// Retrieve a service
var gameManager = ServiceLocator.GetService<IGameManager>();

// Unregister a service
ServiceLocator.UnregisterService<IGameManager>();
```

### Game State Management

The game uses a state machine pattern with the following states:

- **InitializationState**: Handles initial game setup and service initialization
- **MenuState**: Manages main menu interactions
- **GameplayState**: Core tower defense gameplay loop

```csharp
// Change game state
gameManager.ChangeState<MenuState>();

// Listen for state changes
gameManager.OnStateChanged += (previousState, newState) => {
    Debug.Log($"State changed from {previousState?.GetStateName()} to {newState.GetStateName()}");
};
```

### Game Bootstrap

The `GameBootstrap` component initializes all core services and starts the game:

1. Registers the GameManager service
2. Initializes the GameManager
3. Starts in InitializationState
4. Handles application shutdown cleanup

## Getting Started

### Prerequisites

- Unity 2022.3 LTS or later
- Unity Test Framework package (included)

### Setup

1. Open the project in Unity
2. Load the `Bootstrap.unity` scene
3. Press Play to start the game

### Running Tests

#### Unit Tests (Edit Mode)

1. Open Window → General → Test Runner
2. Select "EditMode" tab
3. Click "Run All" to execute unit tests

Tests cover:
- ServiceLocator functionality
- GameManager state transitions
- GameState base class behavior

#### Integration Tests (Play Mode)

1. Open Window → General → Test Runner
2. Select "PlayMode" tab
3. Click "Run All" to execute integration tests

Tests cover:
- Complete system initialization
- Service registration and retrieval
- State transition workflows
- Cleanup and shutdown procedures

## Development Guidelines

### Adding New Services

1. Create an interface extending `IService`
2. Implement the interface with `Initialize()` and `Shutdown()` methods
3. Register the service in `GameBootstrap` or appropriate initialization code

```csharp
public interface IMyService : IService
{
    void DoSomething();
}

public class MyService : IMyService
{
    public bool IsInitialized { get; private set; }
    
    public void Initialize()
    {
        IsInitialized = true;
        // Service initialization logic
    }
    
    public void Shutdown()
    {
        IsInitialized = false;
        // Cleanup logic
    }
    
    public void DoSomething()
    {
        // Service functionality
    }
}
```

### Adding New Game States

1. Create a class extending `GameState`
2. Override `Enter()`, `Update()`, and `Exit()` methods
3. Use `gameManager.ChangeState<YourState>()` to transition

```csharp
public class MyGameState : GameState
{
    public override void Enter()
    {
        base.Enter();
        // State entry logic
    }
    
    public override void Update()
    {
        base.Update();
        // State update logic
    }
    
    public override void Exit()
    {
        base.Exit();
        // State cleanup logic
    }
}
```

### Testing

- Write unit tests for individual components
- Create integration tests for system interactions
- Follow the existing test patterns and naming conventions
- Ensure all tests pass before committing changes

## Architecture Notes

### Design Patterns Used

- **Service Locator**: Centralized service management
- **State Machine**: Game flow control
- **Singleton**: ServiceLocator implementation
- **Template Method**: GameState base class

### Performance Considerations

- ServiceLocator uses thread-safe operations
- State transitions are immediate (no async operations)
- Services are initialized once and reused
- Proper cleanup prevents memory leaks

### Extensibility

The architecture is designed for easy extension:
- New services can be added without modifying existing code
- New game states follow the same pattern
- Testing framework supports both unit and integration tests
- Clear separation of concerns enables parallel development

## Next Steps

This foundation supports the development of:
- Tower defense gameplay mechanics
- Procedural level generation
- UI systems and menus
- Audio and visual effects
- Save/load functionality
- Multiplayer features

Refer to the project documentation in `docs/` for detailed specifications and development roadmap.