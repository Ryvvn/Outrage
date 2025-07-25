using NUnit.Framework;
using Core.Services;
using Core.GameStates;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Unit tests for the GameState base class.
    /// Tests state lifecycle methods and basic functionality.
    /// </summary>
    public class GameStateTests
    {
        private TestableGameState _gameState;
        private MockGameManager _mockGameManager;
        
        [SetUp]
        public void SetUp()
        {
            _gameState = new TestableGameState();
            _mockGameManager = new MockGameManager();
        }
        
        [TearDown]
        public void TearDown()
        {
            _gameState = null;
            _mockGameManager = null;
        }
        
        [Test]
        public void Initialize_WithGameManager_SetsGameManagerReference()
        {
            // Act
            _gameState.Initialize(_mockGameManager);
            
            // Assert
            Assert.AreEqual(_mockGameManager, _gameState.GetGameManager());
        }
        
        [Test]
        public void Enter_NewState_SetsIsActiveToTrue()
        {
            // Arrange
            Assert.IsFalse(_gameState.IsActive);
            
            // Act
            _gameState.Enter();
            
            // Assert
            Assert.IsTrue(_gameState.IsActive);
            Assert.IsTrue(_gameState.WasEnterCalled);
        }
        
        [Test]
        public void Exit_ActiveState_SetsIsActiveToFalse()
        {
            // Arrange
            _gameState.Enter();
            Assert.IsTrue(_gameState.IsActive);
            
            // Act
            _gameState.Exit();
            
            // Assert
            Assert.IsFalse(_gameState.IsActive);
            Assert.IsTrue(_gameState.WasExitCalled);
        }
        
        [Test]
        public void Update_ActiveState_CallsUpdateMethod()
        {
            // Arrange
            _gameState.Enter();
            
            // Act
            _gameState.Update();
            
            // Assert
            Assert.IsTrue(_gameState.WasUpdateCalled);
        }
        
        [Test]
        public void GetStateName_DefaultImplementation_ReturnsClassName()
        {
            // Act
            var stateName = _gameState.GetStateName();
            
            // Assert
            Assert.AreEqual("TestableGameState", stateName);
        }
        
        [Test]
        public void StateLifecycle_FullCycle_CallsMethodsInCorrectOrder()
        {
            // Arrange
            _gameState.Initialize(_mockGameManager);
            
            // Act & Assert - Enter
            _gameState.Enter();
            Assert.IsTrue(_gameState.IsActive);
            Assert.IsTrue(_gameState.WasEnterCalled);
            Assert.IsFalse(_gameState.WasUpdateCalled);
            Assert.IsFalse(_gameState.WasExitCalled);
            
            // Act & Assert - Update
            _gameState.Update();
            Assert.IsTrue(_gameState.IsActive);
            Assert.IsTrue(_gameState.WasEnterCalled);
            Assert.IsTrue(_gameState.WasUpdateCalled);
            Assert.IsFalse(_gameState.WasExitCalled);
            
            // Act & Assert - Exit
            _gameState.Exit();
            Assert.IsFalse(_gameState.IsActive);
            Assert.IsTrue(_gameState.WasEnterCalled);
            Assert.IsTrue(_gameState.WasUpdateCalled);
            Assert.IsTrue(_gameState.WasExitCalled);
        }
        
        [Test]
        public void Enter_CalledMultipleTimes_RemainsActive()
        {
            // Act
            _gameState.Enter();
            _gameState.Enter();
            _gameState.Enter();
            
            // Assert
            Assert.IsTrue(_gameState.IsActive);
            Assert.IsTrue(_gameState.WasEnterCalled);
        }
        
        [Test]
        public void Exit_CalledMultipleTimes_RemainsInactive()
        {
            // Arrange
            _gameState.Enter();
            
            // Act
            _gameState.Exit();
            _gameState.Exit();
            _gameState.Exit();
            
            // Assert
            Assert.IsFalse(_gameState.IsActive);
            Assert.IsTrue(_gameState.WasExitCalled);
        }
        
        [Test]
        public void Update_InactiveState_StillCallsUpdate()
        {
            // Arrange - State is inactive by default
            Assert.IsFalse(_gameState.IsActive);
            
            // Act
            _gameState.Update();
            
            // Assert
            Assert.IsTrue(_gameState.WasUpdateCalled);
        }
    }
    
    #region Test Classes
    
    /// <summary>
    /// Testable implementation of GameState for unit testing.
    /// </summary>
    public class TestableGameState : GameState
    {
        public bool WasEnterCalled { get; private set; }
        public bool WasUpdateCalled { get; private set; }
        public bool WasExitCalled { get; private set; }
        
        public override void Enter()
        {
            base.Enter();
            WasEnterCalled = true;
        }
        
        public override void Update()
        {
            base.Update();
            WasUpdateCalled = true;
        }
        
        public override void Exit()
        {
            base.Exit();
            WasExitCalled = true;
        }
        
        /// <summary>
        /// Exposes the protected gameManager field for testing.
        /// </summary>
        public IGameManager GetGameManager()
        {
            return gameManager;
        }
    }
    
    /// <summary>
    /// Mock implementation of IGameManager for testing.
    /// </summary>
    public class MockGameManager : IGameManager
    {
        public GameState CurrentState { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsTransitioning { get; private set; }
        
        public event System.Action<GameState, GameState> OnStateChanged;
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            IsInitialized = false;
            CurrentState = null;
        }
        
        public void ChangeState<T>() where T : GameState, new()
        {
            var newState = new T();
            ChangeState(newState);
        }
        
        public void ChangeState(GameState newState)
        {
            var previousState = CurrentState;
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
            OnStateChanged?.Invoke(previousState, CurrentState);
        }
    }
    
    #endregion
}